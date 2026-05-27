using PIQI.Components.Models;
using PIQI.Components.SAMs;
using PIQI.Components.Services;
using PIQI_Engine.Server.Services;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PIQI_Engine.Server.Engines
{
    /// <summary>
    /// Engine responsible for processing, scoring, and auditing PIQI messages.
    /// Provides access to configuration, logging, caching, and reference data services.
    /// </summary>
    public class PIQIEngine
    {
        /// <summary>
        /// Application configuration used to access settings and options.
        /// </summary>
        protected readonly IConfiguration _Configuration;

        /// <summary>
        /// Logger used to record information, warnings, and errors during PIQI processing.
        /// </summary>
        protected readonly ILogger<PIQIEngine> _Logger;

        /// <summary>
        /// Caching service for storing and retrieving files used during processing.
        /// </summary>
        protected readonly FileCacheService _Cache;

        /// <summary>
        /// Service used in the SAMs to access reference data such as code systems.
        /// </summary>
        protected readonly SAMService _SAMService;

        /// <summary>
        /// Engine for managing reference data, such as lookups and domain-specific information.
        /// </summary>
        private ReferenceDataEngine _ReferenceDataEngine;

        /// <summary>
        /// Lock object used to synchronize access to the cache to prevent race conditions.
        /// </summary>
        private static object _CacheLock = new object();

        /// <summary>
        /// The registry for managing SAM workers
        /// </summary>
        private readonly SAMWorkerRegistry _samRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PIQIEngine"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="cache">The file cache service.</param>
        /// <param name="samService">The reference data service used in the SAMs.</param>
        /// <param name="referenceDataEngine">The engine used for handling reference data.</param>
        /// <param name="samRegistry">The registry for managing SAM workers.</param>
        public PIQIEngine(
            IConfiguration configuration,
            ILogger<PIQIEngine> logger,
            FileCacheService cache,
            SAMService samService,
            ReferenceDataEngine referenceDataEngine,
            SAMWorkerRegistry samRegistry
            )
        {
            _Configuration = configuration;
            _Logger = logger;
            _Cache = cache;
            _SAMService = samService;
            _ReferenceDataEngine = referenceDataEngine;
            _samRegistry = samRegistry;
        }

        #region Main

        /// <summary>
        /// Processes a PIQI request and generates a result, optionally in audit mode.
        /// </summary>
        /// <param name="piqiRequest">The request object containing the PIQI message details.</param>
        /// <param name="auditMode">Indicates whether the request should generate an audited result.</param>
        /// <param name="evaluation">Optional evaluation file to override or supplement evaluation rubrics.</param>
        /// <returns>A <see cref="PIQIResponse"/> containing the processed message, formatted statistics, and optionally the audited message.</returns>
        /// <exception cref="Exception">Thrown if the message processing fails, reference data cannot be loaded, or validation fails.</exception>
        public async Task<PIQIResponse> PiqiRequestAsync(PIQIRequest piqiRequest, bool auditMode, IFormFile? evaluation = null) 
        {
            // Create final result object
            PIQIResponse result = new PIQIResponse();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Create message object
                PIQIMessage message = new PIQIMessage(piqiRequest);

                // Load the message header
                MessageModel headerResult = MessageModelBuilder.LoadHeader(piqiRequest);
                if (headerResult == null) throw new Exception("Failed to parse message header.");
                message.MessageModel = headerResult;

                //Load reference data: Code system dictionary, Sam list, Entity list, Entity Type list, Criteria list, Value list, and Data Type list
                PIQIReferenceData refDataResult = _ReferenceDataEngine.LoadRefData(piqiRequest.EvaluationRubricMnemonic, evaluation);
                if (refDataResult == null) throw new Exception("Failed to load reference data.");
                message.RefData = refDataResult;

                if (evaluation != null && message.RefData.EvaluationRubric.Mnemonic != null) piqiRequest.EvaluationRubricMnemonic = message.RefData.EvaluationRubric.Mnemonic;

                // Explicitly set Data Type List, Entity Model, and root information to be used when loading messageData content
                message.MessageModel.DataTypeList = refDataResult.DataTypeList;
                message.MessageModel.EntityModel = refDataResult.EntityModel;
                message.MessageModel.RootEntityName = refDataResult.Model.RootEntityName;

                // Get evaluation rubric from evaluation mnemonic and apply it to the message
                EvaluationRubric evaluationRubric = message.RefData.EvaluationRubric;
                if (evaluationRubric == null) throw new Exception("evaluation rubric mnemonic invalid.");

                // Validate the input entity model version mnemonic against the evaluation
                ValidateEntityModelVersionMnemonic(evaluationRubric, piqiRequest.PIQIModelMnemonic);
                message.RefData.EvaluationRubric = evaluationRubric;

                // Load the message content  
                MessageModelBuilder.LoadContent(message.MessageModel, message.RefData);

                // Set the message to be used in the SAMs (needed for reference data)
                _SAMService.Message = message;

                // Process the message
                await ProcessMessageAsync(message);

                // Generate stats
                StatResponse statResponse = message.GenerateStatResponse();

                // Format the information from the stat response into a PIQI stat response
                PIQIStatResponse formattedStatResponse = new PIQIStatResponse(statResponse, message);
                message.FormattedStatResponse = formattedStatResponse;

                // Generate audit message
                if (auditMode)
                {
                    string auditResponse = message.GenerateAuditResponse();
                    result.AuditedMessage = auditResponse;
                }

                stopwatch.Stop();
                // Set result succeeded
                result.Succeed(formattedStatResponse, stopwatch);
            }
            catch (Exception ex)
            {
                // Fail result with exception message
                _Logger.LogError(ex, "PiqiRequestAsync: " + ex.Message);
                result.Fail(ex);
            }
            return result;
        }

        #endregion

        #region Validation
        // Validates the message model against the evaluation rubric model
        private void ValidateEntityModelVersionMnemonic(EvaluationRubric evaluationRubric, string messagePIQIModelMnemonic)
        {
            try
            {
                if (messagePIQIModelMnemonic == null || string.IsNullOrWhiteSpace(messagePIQIModelMnemonic)) throw new Exception("Missing model version mnemonic.");
                var pattern = @"^(.*?)_V(\d+)(?:_(.*))?$";
                string? messageModelMnemonic, messageVersion, messageExtension = null;
                string? evaluationModelMnemonic, evaluationVersion, evaluationExtension = null;

                // Split the message model mnemonic into parts
                var messageMatch = Regex.Match(messagePIQIModelMnemonic, pattern);
                if (messageMatch.Success)
                {
                    messageModelMnemonic = messageMatch.Groups[1].Value; // "model_x_y_z"
                    messageVersion = messageMatch.Groups[2].Value; // "123"
                    messageExtension = messageMatch.Groups[3].Success ? messageMatch.Groups[3].Value : null; // optional
                }
                else throw new Exception("Invalid message model.");

                // Split the evaluation model mnemonic into parts
                var evalMatch = Regex.Match(evaluationRubric.Model.Mnemonic ?? "", pattern);
                if (evalMatch.Success)
                {
                    evaluationModelMnemonic = evalMatch.Groups[1].Value; // "model_x_y_z"
                    evaluationVersion = evalMatch.Groups[2].Value; // "123"
                    evaluationExtension = evalMatch.Groups[3].Success ? evalMatch.Groups[3].Value : null; // optional
                }
                else throw new Exception("Invalid evaluation model.");

                // Verify the models match
                if (!messageModelMnemonic.Equals(evaluationModelMnemonic, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Message model mnemonic does not match the evaluation rubric.");
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #region Message Processing
        // Creates the evaluation manager from the messag emodel and processes the message
        private async Task ProcessMessageAsync(PIQIMessage message)
        {
            try
            {
                // Build the eval model
                message.EvaluationManager.Load(message.RefData.EntityModel, message.MessageModel);

                // Get the root eval item
                EvaluationItem rootEvalItem = message.EvaluationManager.RootItem;

                // Process its children (all classes, elements and attrs)
                await ProcessRootAsync(message, rootEvalItem);
            }
            catch
            {
                throw;
            }
        }

        // Loops through all class level evaluation items then processes all root level criteria
        private async Task ProcessRootAsync(PIQIMessage message, EvaluationItem rootEvaluationItem)
        {
            try
            {
                // Process all root classes
                foreach (EvaluationItem classEvalItem in rootEvaluationItem.ChildDict.Values.OrderBy(t => t.Entity.Name))
                {
                    await ProcessClassAsync(message, classEvalItem);
                }

                // Process any criteria on the root itself
                // Get criteria
                List<EvaluationCriterion> critList = message.GetCriteriaList(rootEvaluationItem.Entity.Mnemonic);

                foreach (EvaluationCriterion criterion in critList.OrderBy(t => t.Sequence))
                {
                    await ProcessCriteriaSAMAsync(message, rootEvaluationItem, criterion);
                }
            }
            catch
            {
                throw;
            }
        }

        // Loops through all element level evaluation items then processes all class level criteria
        private async Task ProcessClassAsync(PIQIMessage message, EvaluationItem classEvaluationItem)
        {
            try
            {
                // Process class elements
                foreach (EvaluationItem elementEvalItem in classEvaluationItem.ChildDict.Values.OrderBy(t => t.ElementSequence))
                {
                    await ProcessElementAsync(message, elementEvalItem);
                }

                // Get criteria for this class
                List<EvaluationCriterion> critList = message.GetCriteriaList(classEvaluationItem.Entity.Mnemonic);

                foreach (EvaluationCriterion criterion in critList.OrderBy(t => t.Sequence))
                {
                    await ProcessCriteriaSAMAsync(message, classEvaluationItem, criterion);
                }
            }
            catch
            {
                throw;
            }
        }

        // Loops through all attribute level evaluation items, there are no element level criteria to process
        private async Task ProcessElementAsync(PIQIMessage message, EvaluationItem elementEvaluationItem)
        {
            try
            {
                // Process element attrs
                foreach (EvaluationItem attrEvalItem in elementEvaluationItem.ChildDict.Values.OrderBy(t => t.Entity.Name))
                {
                    await ProcessAttributeAsync(message, attrEvalItem);
                }

                // Get criteria for this element
                List<EvaluationCriterion> critList = message.GetCriteriaList(elementEvaluationItem.Entity.Mnemonic);

                foreach (EvaluationCriterion criterion in critList.OrderBy(t => t.Sequence))
                {
                    await ProcessCriteriaSAMAsync(message, elementEvaluationItem, criterion);
                }
            }
            catch
            {
                throw;
            }
        }

        // Loops through all attribute level criteria and processes them
        private async Task ProcessAttributeAsync(PIQIMessage message, EvaluationItem attributeEvaluationItem)
        {
            try
            {
                // Get criteria for this attribute
                List<EvaluationCriterion> critList = message.GetCriteriaList(attributeEvaluationItem.Entity.Mnemonic);

                foreach (EvaluationCriterion criterion in critList.OrderBy(t => t.Sequence))
                {
                    await ProcessCriteriaSAMAsync(message, attributeEvaluationItem, criterion);
                }
            }
            catch
            {
                throw;
            }
        }

        //  Validates the SAM parameters, triggers the conditional SAM and its dependencies, then triggers the evaluation criteria SAM and its dependencies
        private async Task ProcessCriteriaSAMAsync(PIQIMessage message, EvaluationItem evaluationItem, EvaluationCriterion evaluationCriterion)
        {
            try
            {
                // Get SAM from evaluation criterion
                SAM? criteriaSAM = message.RefData.GetSAM(evaluationCriterion.SAMMnemonic);
                if (criteriaSAM == null)
                    throw new Exception($"Criteria SAM{(evaluationCriterion.SAMMnemonic != null ? " (" + evaluationCriterion.SAMMnemonic + ")" : "")} missing from SAM reference list or invalid.");

                // Create the evaluation result from the sam, criterion, and evaluation item
                EvaluationResult evaluationResult = message.EvaluationManager.CreateEvalResult(evaluationItem, evaluationCriterion, criteriaSAM, false, false);

                // Check if evaluation in the evaluation criteria is valid, skip if not
                SAM? evalValidSam = message.RefData.GetSAM("EVAL_ISVALID");
                if (evalValidSam == null)
                    throw new Exception($"Evaluation validity SAM (EVAL_ISVALID missing) from SAM reference list or invalid.");

                if (!EvalIsValid(evaluationCriterion, criteriaSAM))
                {
                    _Logger.Log(LogLevel.Warning, $"Invalid evaluation criteria: {evaluationCriterion.Description}");
                    evaluationResult.Skip(evalValidSam);
                }

                // Check for a conditional SAM if processing state is still pending. If conditional SAM exists, process it prior to running the SAM in the evaluation criteria
                if (evaluationResult.EvalPending && evaluationCriterion.ConditionalSAM != null)
                {
                    // Get SAM from evaluation criterion
                    SAM? conditionalCriteriaSAM = message.RefData.GetSAM(evaluationCriterion.ConditionalSAM);
                    if (conditionalCriteriaSAM == null)
                        throw new Exception($"Conditional SAM{(evaluationCriterion.ConditionalSAM != null ? " (" + evaluationCriterion.ConditionalSAM + ")" : "")} missing from SAM reference list or invalid.");

                    // Create the conditional evaluation result
                    EvaluationResult conditionalEvaluationResult = new EvaluationResult(evaluationItem, evaluationCriterion, conditionalCriteriaSAM, true, false);

                    // Process conditional SAM and its prerequisites
                    await ProcessSAMAsync(message, conditionalEvaluationResult, evaluationCriterion.ConditionalSAM, true);

                    // Skip current criteria SAM if conditional SAM fails
                    if (conditionalEvaluationResult.EvalFailed)
                        evaluationResult.Skip(conditionalCriteriaSAM, conditionalEvaluationResult.Reason);
                }

                // Check if processing state is still pending. Process this SAM if so
                if (evaluationResult.EvalPending)
                {
                    await ProcessSAMAsync(message, evaluationResult, criteriaSAM.Mnemonic, false);
                }

                // Add the result to our hash
                evaluationResult.EvaluationItem.AddFullResult(evaluationResult);
            }
            catch
            {
                throw;
            }
        }

        #region SAM Processing
        // Gets evualation parameters and then processes the SAM and its dependencies
        private async Task ProcessSAMAsync(PIQIMessage message, EvaluationResult evaluationResult, string initalSAMMNemonic, bool isConditional)
        {
            try
            {
                if (evaluationResult == null) throw new Exception("Invalid evaluation result item.");
                // Stack of SAMs used to process the prerequisite SAMs in order
                Stack<SAM> dependencySAMStack = new Stack<SAM>();
                var dependencySAMMnemonic = initalSAMMNemonic;

                // Get the chain of prerequisite SAMs needed for the SAM in evaluationCriterion
                while (dependencySAMMnemonic != null)
                {
                    // Get the SAM matching the prerequisite mnemonic and add it to the stack of SAMS
                    SAM dependencySAM = message.RefData.GetSAM(dependencySAMMnemonic);
                    if (dependencySAM == null) throw new Exception($"Dependency SAM {dependencySAMMnemonic} not found.");

                    // Push the Dependency sam onto the stack to process later
                    dependencySAMStack.Push(dependencySAM);

                    // Set prerequisiteSAMMnemonic to the prerequisiteSAM's prerequisite SAM 
                    dependencySAMMnemonic = dependencySAM.PrerequisiteSAMMnemonic;
                }
                // Process the SAMs in the prerequisiteStack in order 
                while (dependencySAMStack.Count > 0)
                {
                    // Get SAM and PIQISAM for processing
                    SAM processingSAM = dependencySAMStack.Pop();

                    // Criteria parameter
                    List<EvaluationCriteriaParameter>? evaluationCriteriaParameters = null;
                    string? evaluationCriteriaProcessingURL = null;
                    string? dataMnemonic = null;
                    // Check for parameters (only necesary if the SAM is not dependent)
                    if (processingSAM.Parameters != null && processingSAM.Parameters.Count > 0 && processingSAM.Mnemonic == initalSAMMNemonic)
                    {
                        // Get the evaluation criteria and evaluation criteria parameters
                        evaluationCriteriaParameters = isConditional ? evaluationResult.Criterion?.ConditionalSAMParameters?.ToList() : evaluationResult.Criterion?.SAMParameters?.ToList();
                        evaluationCriteriaProcessingURL = evaluationResult.Criterion?.ProcessingURL;

                        // Get the dataMnemonic specified in the param, if necessary
                        if (processingSAM.Mnemonic == "attr_is_in_value_list")
                            dataMnemonic = evaluationCriteriaParameters?.FirstOrDefault()?.ParameterValue;
                    }
                    else if (processingSAM.Mnemonic == "attr_is_uom")
                        dataMnemonic = "UCUM";

                    // If we're using value data, ensure the appropriate data is loaded
                    if (!string.IsNullOrEmpty(dataMnemonic) && message.RefData.GetValueList(dataMnemonic) == null)
                        throw new Exception("Failed to load value data for [" + dataMnemonic + "]");

                    // Get the executable SAM
                    ISAMWorker samWorker = _samRegistry.CreateWorker(processingSAM.Mnemonic, processingSAM, _SAMService);

                    // We didn't find anything that matches the mnemonic, we get back the default sam - log a warning and continue trying to execute the SAM. 
                    if (samWorker.Mnemonic == "default")
                        _Logger.Log(LogLevel.Warning, "SAM [" + processingSAM.Mnemonic + "] wasn't found in the cache. Executing default SAM instead.");

                    PIQISAMRequest? samRequest = new PIQISAMRequest();
                    PIQISAMResponse? samResult = null;

                    // Create SAM request object
                    samRequest.EvaluationObject = evaluationResult.EvaluationItem;
                    if (evaluationCriteriaParameters != null && evaluationCriteriaParameters.Count > 0)
                    {
                        // Add processing URL as a parameter
                        if (evaluationCriteriaProcessingURL != null)
                            samRequest.AddParameter("Processing URL", evaluationCriteriaProcessingURL);
                        for (int i = 0; i < evaluationCriteriaParameters.Count; i++)
                        {
                            EvaluationCriteriaParameter ecp = evaluationCriteriaParameters[i];
                            SAMParameter? samParameter = processingSAM.Parameters?.Where(t => t.Mnemonic == ecp.SamParameterMnemonic).FirstOrDefault();
                            if (samParameter == null || samParameter.Mnemonic == null || ecp.ParameterValue == null)
                                _Logger.Log(LogLevel.Warning, $"Invalid SAM parameter object in {evaluationResult.Criterion.Entity}: {samParameter?.Mnemonic}, {ecp.ParameterValue}");
                            else
                            {
                                samRequest.AddParameter(samParameter.Mnemonic, ecp.ParameterValue);
                            }
                        }
                    }

                    //Execute the SAM
                    samResult = await samWorker.EvaluateAsync(samRequest);

                    // Validate that we ran successfully
                    if (samResult == null || samResult.ResultState == SAMResultStateEnum.ERRORED)
                        throw new Exception($"{processingSAM.Mnemonic} failed to process {(samResult != null ? ": " + samResult?.ErrorMessage : "")}");

                    // Fail the criteria SAM if it or one of its dependencies fails
                    if (samResult.Failed)
                    {
                        evaluationResult.Fail(processingSAM, samResult.FailReason);
                        break;
                    }

                }

                // No exceptions means the method succeeded
                if (evaluationResult.EvalResult != ProcessStateEnum.Failed) evaluationResult.Pass();
            }
            catch
            {
                throw;
            }
        }

        // Validates the evaluation criteria by checking for necesary SAM parameters
        private bool EvalIsValid(EvaluationCriterion evaluationCriterion, SAM sam)
        {
            try
            {
                // Default state is passed
                bool passed = true;

                // If there are parameters, go through each SAM parameter, find the matching evaluation criteria parameter and ensure it's populated
                if (sam.Parameters != null)
                {
                    foreach (SAMParameter samParameter in sam.Parameters)
                    {
                        // If there are no parameters in the criterion, the evaluation is invalid
                        if (evaluationCriterion.SAMParameters != null)
                        {
                            EvaluationCriteriaParameter? evaluationCriteriaParameter = evaluationCriterion.SAMParameters.FirstOrDefault(ecsp => ecsp.ParameterName == samParameter.Name);
                            if (evaluationCriteriaParameter == null || string.IsNullOrEmpty(evaluationCriteriaParameter.ParameterValue)) passed = false;
                        }
                        else passed = false;
                    }
                }

                return passed;
            }
            catch
            {
                throw;
            }
        }
        #endregion

        #endregion
    }
}
