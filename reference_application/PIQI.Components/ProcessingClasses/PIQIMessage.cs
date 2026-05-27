using Newtonsoft.Json.Linq;
using PIQI.Components.Services;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a PIQI message and its associated evaluation context.
    /// </summary>
    public class PIQIMessage
    {
        #region Properties

        /// <summary>
        /// The request that initiated this PIQI message.
        /// </summary>
        public PIQIRequest PIQIRequest { get; set; }

        /// <summary>
        /// Reference data used during evaluation.
        /// </summary>
        public PIQIReferenceData RefData { get; set; }

        /// <summary>
        /// The message model that represents the structure and content of this message.
        /// </summary>
        public MessageModel MessageModel { get; set; }

        /// <summary>
        /// Gets or sets the evaluation manager responsible for executing and tracking evaluation processes.
        /// </summary>
        public EvaluationManager EvaluationManager { get; set; }

        /// <summary>
        /// Statistical results generated from processing this message.
        /// </summary>
        public StatResponse StatResponse { get; set; }

        /// <summary>
        /// Formatted statistical result for this message.
        /// </summary>
        public PIQIStatResponse FormattedStatResponse { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PIQIMessage"/> class with the specified request.
        /// </summary>
        /// <param name="piqiRequest">The PIQI request associated with this message.</param>
        public PIQIMessage(PIQIRequest piqiRequest)
        {
            PIQIRequest = piqiRequest;
            EvaluationManager = new EvaluationManager();
        }

        #endregion

        #region Stat Methods

        /// <summary>
        /// Generates the statistical result for this PIQI message by processing all applicable PIQI SAMs.
        /// </summary>
        /// <returns>A <see cref="StatResponse"/> containing aggregated scoring, pass/fail, and informational results.</returns>
        public StatResponse GenerateStatResponse()
        {
            // Create a new stat result object
            StatResponse = new StatResponse();

            try
            {
                // Process results for each scorable evaluation result
                foreach (var evaluationResult in EvaluationManager.EvaluationResultDict.Values)
                {
                    // Ignore conditional or dependent results
                    if (evaluationResult.IsConditional || evaluationResult.IsDependent) continue;

                    StatResponse.ProcessResult(evaluationResult, RefData);
                }

                // Calc classes
                foreach (EvaluationItem classItem in EvaluationManager.EvaluationItemDict.Values.Where(t => t.ItemType == EntityItemTypeEnum.Class))
                {
                    StatResponseClass statClass = new StatResponseClass(classItem.Entity.Mnemonic, classItem?.MessageItem?.ChildDict?.Count() ?? 0);
                    List<StatResponseElement>? elementResponseList = StatResponse.ElementDict?.Values?.Where(t => t.ClassMnemonic == statClass.ClassMnemonic).ToList();
                    if (elementResponseList != null)
                    {
                        statClass.Calc(elementResponseList);
                        StatResponse.ClassDict.Add(statClass.Key, statClass);
                    }
                }
            }
            catch
            {
                throw;
            }

            // Return the generated statistical result
            return StatResponse;
        }

        #endregion

        #region Audit Methods

        /// <summary>
        /// Generates a JSON-formatted audit result for the current PIQI message.
        /// </summary>
        /// <returns>A JSON string representing the audit result.</returns>
        public string GenerateAuditResponse()
        {
            try
            {
                // Create root message object
                JObject msg = new JObject();

                // Add header info
                msg.Add("EntityModelMnemonic", MessageModel.Header.EntityModelMnemonic);
                msg.Add("DataProviderID", PIQIRequest.DataProviderID ?? MessageModel.Header.ProviderName);
                msg.Add("DataSourceID", PIQIRequest.DataSourceID ?? MessageModel.Header.DataSourceName);
                msg.Add("MessageID", PIQIRequest.MessageID ?? MessageModel.Header.ClientMessageID);

                // Add message-level audit info
                Audit_AddMessageInfo(msg, FormattedStatResponse);

                // Process the message model recursively
                Audit_ProcessMessageModelItem(EvaluationManager.RootItem, msg);

                return msg.ToString();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Recursively processes a message model item and adds audit information to the JSON node.
        /// </summary>
        /// <param name="item">The message model item to process.</param>
        /// <param name="parentItem">The optional model item parent to process (Needed for element sequence on attribute).</param>
        /// <param name="parentNode">The parent JSON node to attach audit information.</param>
        /// <param name="entity">The optional entity to process. Used in tandem with item to include processed entities without item data</param>

        private void Audit_ProcessMessageModelItem(EvaluationItem evaluationItem, JToken parentNode)
        {
            try
            {
                JObject? elementNode = null;
                string itemName = evaluationItem?.Entity?.FieldName ?? evaluationItem?.Entity?.Name ?? "UNKNOWN";

                if (evaluationItem.ItemType == EntityItemTypeEnum.Root)
                {
                    JObject itemNode = Utility.JSON_AddObject((JObject)parentNode, itemName);

                    foreach (EvaluationItem classEvaluationItem in evaluationItem.ChildDict?.Values?.OrderBy(e => e.Entity?.Name).ToList() ?? [])
                        Audit_ProcessMessageModelItem(classEvaluationItem, itemNode);
                }
                else if (evaluationItem.ItemType == EntityItemTypeEnum.Class)
                {
                    JArray itemNode = Utility.JSON_AddArray((JObject)parentNode, itemName);

                    foreach (EvaluationItem elementEvaluationItem in evaluationItem.ChildDict?.Values?.OrderBy(e => e.Entity?.Name).ToList() ?? [])
                        Audit_ProcessMessageModelItem(elementEvaluationItem, itemNode);
                }
                else if (evaluationItem.ItemType == EntityItemTypeEnum.Element)
                {
                    JObject itemNode = Utility.JSON_AddObject((JArray)parentNode, itemName);
                    elementNode = itemNode;

                    foreach (EvaluationItem attributeEvaluationItem in evaluationItem.ChildDict?.Values?.OrderBy(e => e.Entity?.Name).ToList() ?? [])
                        Audit_ProcessMessageModelItem(attributeEvaluationItem, itemNode);
                }
                else if (evaluationItem.ItemType == EntityItemTypeEnum.Attribute)
                {
                    JObject? attrAuditItem = new JObject();
                    if (evaluationItem.MessageItem != null)
                    {
                        JToken? attrDataItem = null;

                        // Handle leaf elements according to their data type
                        if (evaluationItem.Entity?.EntityType.EntityTypeValue == EntityDataTypeEnum.CC)
                            attrDataItem = Utility.JSON_AddCodeableConceptObject((CodeableConcept)evaluationItem.MessageItem?.MessageData);
                        else if (evaluationItem.Entity?.EntityType.EntityTypeValue == EntityDataTypeEnum.OBSVAL)
                            attrDataItem = Utility.JSON_AddValueObject((Value)evaluationItem.MessageItem?.MessageData);
                        else if (evaluationItem.Entity?.EntityType.EntityTypeValue == EntityDataTypeEnum.RV)
                            attrDataItem = Utility.JSON_AddRefRangeObject((ReferenceRange)evaluationItem.MessageItem?.MessageData);
                        else
                            attrDataItem = (evaluationItem.MessageItem?.MessageData?.Text ?? "");

                        if (attrDataItem == null) throw new Exception("Attribute information not found.");
                        attrAuditItem.Add("data", attrDataItem);
                    }

                    Audit_AddAttributeInfo(evaluationItem, attrAuditItem);

                    ((JObject)parentNode).Add(itemName, attrAuditItem);
                }

                if (elementNode != null)
                    Audit_AddElementInfo(evaluationItem, elementNode);
                else if (elementNode != null) throw new Exception("Element information not found.");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Adds message-level audit information such as scoring summary.
        /// </summary>
        /// <param name="parent">Parent JSON node to attach the audit info.</param>
        /// <param name="statResponse">The formatted statistical result for this message.</param>
        public void Audit_AddMessageInfo(JObject parent, PIQIStatResponse statResponse)
        {
            try
            {
                JObject audit = Utility.JSON_AddObject(parent, "Audit");

                audit.Add("messageNumerator", statResponse.MessageResults.Numerator.ToString());
                audit.Add("messageDenominator", statResponse.MessageResults.Denominator.ToString());
                audit.Add("messageScore", (statResponse.MessageResults.Denominator > 0 ? ((int)((double)statResponse.MessageResults.Numerator / (double)statResponse.MessageResults.Denominator * 100)) : 0).ToString());
                audit.Add("messageNumeratorWeighted", statResponse.MessageResults.WeightedNumerator.ToString());
                audit.Add("messageDenominatorWeighted", statResponse.MessageResults.WeightedDenominator.ToString());
                audit.Add("messageScoreWeighted", (statResponse.MessageResults.WeightedDenominator > 0 ? (int)((double)statResponse.MessageResults.WeightedNumerator / (double)statResponse.MessageResults.WeightedDenominator * 100) : 0).ToString());
                audit.Add("messageCriticalFailureCount", statResponse.MessageResults.CriticalFailureCount.ToString());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Adds element-level audit information for a specific message model item.
        /// </summary>
        /// <param name="parent">The JSON node to attach the element audit info.</param>
        /// <param name="elementItem">The message model element item to audit.</param>
        /// <returns>The audit JSON node for the element.</returns>
        private JObject? Audit_AddElementInfo(EvaluationItem elementEvaluationItem, JObject parent)
        {
            JObject? auditNode = null;

            try
            {
                List<EvaluationResult> elementEvaluationResultList = EvaluationManager.EvaluationResultDict.Values
               .Where(er => er.EvaluationItem.ElementEntityMnemonic == elementEvaluationItem.Entity.Mnemonic && er.EvaluationItem.ElementSequence == elementEvaluationItem.ElementSequence).ToList();

                if (elementEvaluationResultList.Count > 0)
                {
                    auditNode = Utility.JSON_AddObject(parent, "elementAudit");

                    int denominator = 0;
                    int denominatorWeight = 0;
                    int numerator = 0;
                    int numeratorWeight = 0;
                    int criticalFailureCount = 0;

                    foreach (EvaluationResult result in elementEvaluationResultList
                                 .Where(er => !er.IsDependent && !er.IsConditional)
                                 .OrderBy(er => er.EntityMnemonic).ThenBy(er => er.SamDisplayName))
                    {
                        if (result.EvalPassed)
                        {
                            if (result.IsScoring)
                            {
                                denominator++;
                                denominatorWeight += result.ScoringWeight;
                                numerator++;
                                numeratorWeight += result.ScoringWeight;
                            }
                        }
                        else if (result.EvalFailed)
                        {
                            if (result.IsScoring)
                            {
                                denominator++;
                                denominatorWeight += result.ScoringWeight;
                                if (result.IsCritical) criticalFailureCount++;
                            }
                        }
                    }

                    double elementScore = denominator > 0 ? (double)numerator / denominator * 100 : 0;
                    double elementScoreW = denominatorWeight > 0 ? (double)numeratorWeight / denominatorWeight * 100 : 0;

                    auditNode.Add("elementScore", ((int)elementScore).ToString());
                    auditNode.Add("elementScoreWeighted", ((int)elementScoreW).ToString());
                    auditNode.Add("elementCriticalFailureCount", criticalFailureCount.ToString());
                    auditNode.Add("elementNumerator", numerator.ToString());
                    auditNode.Add("elementDenominator", denominator.ToString());
                }
            }
            catch
            {
                throw;
            }
            return auditNode;
        }

        /// <summary>
        /// Adds element-level audit information for a specific message model item.
        /// </summary>
        /// <param name="parent">The JSON node to attach the element audit info.</param>
        /// <param name="attributeItem">The message model element item to audit.</param>
        /// <returns>The audit JSON node for the element.</returns>
        private JObject? Audit_AddAttributeInfo(EvaluationItem attributeEvaluationItem, JObject parent)
        {
            JObject? auditNode = null;

            try
            {
                List<EvaluationResult> attributeEvaluationResultList = EvaluationManager.EvaluationResultDict.Values
                .Where(er => er.EvaluationItem == attributeEvaluationItem).ToList();

                if (attributeEvaluationResultList.Count > 0)
                {
                    auditNode = Utility.JSON_AddObject(parent, "attributeAudit");
                    JObject attributeNode = Utility.JSON_AddObject(auditNode, "scoringData");
                    JArray assessmentNode = Utility.JSON_AddArray(auditNode, "assessmentItems");
                    JArray informationalNode = Utility.JSON_AddArray(auditNode, "InformationalItems");

                    int denominator = 0;
                    int denominatorWeight = 0;
                    int numerator = 0;
                    int numeratorWeight = 0;
                    int criticalFailureCount = 0;

                    foreach (EvaluationResult result in attributeEvaluationResultList
                                 .Where(er => !er.IsDependent && !er.IsConditional)
                                 .OrderBy(er => er.EntityMnemonic).ThenBy(er => er.SamDisplayName))
                    {
                        JObject attrNode = result.IsScoring ? Utility.JSON_AddObject(assessmentNode) : Utility.JSON_AddObject(informationalNode);
                        attrNode.Add("attributeMnemonic", result.EntityMnemonic);
                        attrNode.Add("attributeName", result.EntityName ?? "");
                        attrNode.Add("assessment", result.SamDisplayName ?? RefData.GetSAM(result.SamMnemonic ?? "")?.Name ?? result.SamMnemonic);
                        attrNode.Add("effect", result.IsScoring ? "Scoring" : "Informational");

                        if (result.EvalPassed)
                        {
                            attrNode.Add("status", "Passed");
                            if (result.IsScoring)
                            {
                                denominator++;
                                denominatorWeight += result.ScoringWeight;
                                numerator++;
                                numeratorWeight += result.ScoringWeight;
                            }
                            attrNode.Add("reason", "");
                        }
                        else if (result.EvalSkipped)
                        {
                            attrNode.Add("status", "Skipped");
                            attrNode.Add("reason", result.Reason ?? (RefData.GetSAM(result.SkipSamMnemonic ?? "")?.FailName ?? RefData.GetSAM(result.SkipSamMnemonic ?? "")?.Name));
                        }
                        else if (result.EvalFailed)
                        {
                            attrNode.Add("status", "Failed");
                            attrNode.Add("reason", result.Reason ?? 
                                (result.FailSamMnemonic == result.Criterion.SAMMnemonic ? 
                                (result.Criterion.FailureNameOverride ?? result.Criterion.SamNameOverride ?? RefData.GetSAM(result.FailSamMnemonic ?? "")?.FailName ?? RefData.GetSAM(result.FailSamMnemonic ?? "")?.Name) : 
                                (RefData.GetSAM(result.FailSamMnemonic ?? "")?.FailName ?? RefData.GetSAM(result.FailSamMnemonic ?? "")?.Name)));
                            if (result.IsScoring)
                            {
                                denominator++;
                                denominatorWeight += result.ScoringWeight;
                                if (result.IsCritical) criticalFailureCount++;
                            }
                        }
                    }

                    double attributetScore = denominator > 0 ? (double)numerator / denominator * 100 : 0;
                    double attributeScoreW = denominatorWeight > 0 ? (double)numeratorWeight / denominatorWeight * 100 : 0;

                    attributeNode.Add("attributeScore", ((int)attributetScore).ToString());
                    attributeNode.Add("attributeScoreWeighted", ((int)attributeScoreW).ToString());
                    attributeNode.Add("attributeCriticalFailureCount", criticalFailureCount.ToString());
                    attributeNode.Add("attributeNumerator", numerator.ToString());
                    attributeNode.Add("attributeDenominator", denominator.ToString());
                }
            }
            catch
            {
                throw;
            }
            return auditNode;
        }

        #endregion

        #region Criteria 

        /// <summary>
        /// Retrieves all evaluation criteria that match the specified entity mnemonic from the reference data.
        /// </summary>
        /// <param name="entityMnemonic">
        /// The mnemonic identifier of the entity used to filter evaluation criteria.
        /// </param>
        /// <returns>
        /// A <see cref="List{EvaluationCriterion}"/> containing all criteria associated with the specified entity.
        /// Returns an empty list if no matching criteria are found.
        /// </returns>
        public List<EvaluationCriterion> GetCriteriaList(string entityMnemonic)
        {
            return RefData.EvaluationRubric.Criteria.Where(c => c.Entity.Equals(entityMnemonic)).ToList();
        }

        #endregion
    }
}
