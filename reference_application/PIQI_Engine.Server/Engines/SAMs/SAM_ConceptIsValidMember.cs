using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM (Semantic Assessment Module) that evaluates whether a <see cref="CodeableConcept"/>
    /// contains at least one interoperable coding based on a provided list of valid code systems.
    /// </summary>
    public class SAM_ConceptIsValidMember : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ConceptIsValidMember"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ConceptIsValidMember(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the provided <see cref="MessageModelItem"/> contains
        /// a <see cref="CodeableConcept"/> with at least one coding that matches
        /// the list of valid code systems.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> containing a <see cref="CodeableConcept"/>.</item>
        ///   <item>Optional entries in <see cref="PIQISAMRequest.ParmList"/> where the second parameter is a delimited string of valid code systems.</item>
        /// </list>
        /// </param>
        /// <param name="referenceData">The reference data containing recognized code systems.</param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if at least one coding in the <see cref="CodeableConcept"/> is interoperable.</item>
        ///   <item><c>Failed</c> if no codings are interoperable.</item>
        ///   <item><c>Errored</c> if the input data or SAM parameters are invalid or an exception occurs.</item>
        /// </list>
        /// </returns>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // Set the message model item
                EvaluationItem evaluationItem = (EvaluationItem)request.EvaluationObject;
                MessageModelItem item = evaluationItem?.MessageItem;

                // Since we're an attr sam we want to play with the item's message data
                BaseText data = (BaseText)item.MessageData;

                // Validate the data format
                if (data is not CodeableConcept codeableConcept)
                    throw new Exception("CodeableConceptIsValidMember expects a CodeableConcept value.");

                // Get our parameter for valid code systems
                if (request.ParmList == null) throw new Exception("Parameter list was not supplied");
                Tuple<string, string> arg1 = request.ParmList.Where(t => t.Item1 == "CODE_SYSTEM_CSV").FirstOrDefault();
                if (arg1 == null) throw new Exception("[Code System List] parameter not found");
                string setMnemonic = arg1.Item2;

                // Split parameter into list
                List<string> systemsList = Utility.Split(setMnemonic);

                // Call FHIR server if not called already
                if (!codeableConcept.FHIRServerCalled)
                    await _SAMService.LookupCodeAsync(codeableConcept);

                // Update all codings against the list
                _SAMService.UpdateInteroperability(codeableConcept.CodingList, systemsList);

                // Evaluate
                passed = codeableConcept.CodingList.Any(t => t.IsInteroperable);

                // Update result
                result.Done(passed);
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Gets the mnemonic code for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "CONCEPT_ISVALIDMEMBER";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
