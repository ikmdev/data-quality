namespace PIQI.Components.Models
{
    /// <summary>
    /// Holds statistical results for a SAM (Scoring and Assessment Method) execution.
    /// Tracks counts for scoring, weighted scoring, informational, skipped, failed, and critical failures.
    /// </summary>
    public class StatResponse
    {
        #region Properties

        /// <summary> Total number of scoring items processed. </summary>
        public int ScoringTotalCount { get; set; }

        /// <summary> Number of scoring items skipped. </summary>
        public int ScoringSkipCount { get; set; }

        /// <summary> Number of scoring items processed. </summary>
        public int ScoringProcCount { get; set; }

        /// <summary> Number of scoring items passed. </summary>
        public int ScoringPassCount { get; set; }

        /// <summary> Number of scoring items failed. </summary>
        public int ScoringFailCount { get; set; }

        /// <summary> Number of critical failures encountered. </summary>
        public int CriticalFailureCount { get; set; }

        /// <summary> Total number of weighted scoring items. </summary>
        public int WeightedTotalCount { get; set; }

        /// <summary> Number of weighted scoring items skipped. </summary>
        public int WeightedSkipCount { get; set; }

        /// <summary> Number of weighted scoring items processed. </summary>
        public int WeightedProcCount { get; set; }

        /// <summary> Number of weighted scoring items passed. </summary>
        public int WeightedPassCount { get; set; }

        /// <summary> Number of weighted scoring items failed. </summary>
        public int WeightedFailCount { get; set; }

        /// <summary> Total number of informational items. </summary>
        public int InfoTotalCount { get; set; }

        /// <summary> Number of informational items skipped. </summary>
        public int InfoSkipCount { get; set; }

        /// <summary> Number of informational items processed. </summary>
        public int InfoProcCount { get; set; }

        /// <summary> Number of informational items passed. </summary>
        public int InfoPassCount { get; set; }

        /// <summary> Number of informational items failed. </summary>
        public int InfoFailCount { get; set; }

        /// <summary> Dictionary of classes with their statistical results, keyed by entity type mnemonic. </summary>
        public Dictionary<string, StatResponseClass> ClassDict { get; set; }

        /// <summary> Dictionary of elements with their statistical results, keyed by entity type mnemonic and sequence. </summary>
        public Dictionary<string, StatResponseElement> ElementDict { get; set; }

        /// <summary> Dictionary of critical failures, keyed by EntityMnemonic|SAMMnemonic|FailSAMMnemonic. </summary>
        public Dictionary<string, StatResponseCriticalFailure> CritcalFailureDict { get; set; }

        /// <summary> Dictionary of informational results, keyed by EntityMnemonic|SAMMnemonic. </summary>
        public Dictionary<string, StatResponseInformational> InformationalDict { get; set; }

        /// <summary> Dictionary of skipped SAMs, keyed by EntityMnemonic|SAMMnemonic|SkipSAMMnemonic. </summary>
        public Dictionary<string, StatResponseSkip> SkipDict { get; set; }

        /// <summary> Dictionary of failed SAMs, keyed by EntityMnemonic|SAMMnemonic|FailSAMMnemonic. </summary>
        public Dictionary<string, StatResponseFail> FailDict { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="StatResponse"/> with empty dictionaries.
        /// </summary>
        public StatResponse()
        {
            ClassDict = new Dictionary<string, StatResponseClass>();
            ElementDict = new Dictionary<string, StatResponseElement>();
            CritcalFailureDict = new Dictionary<string, StatResponseCriticalFailure>();
            InformationalDict = new Dictionary<string, StatResponseInformational>();
            SkipDict = new Dictionary<string, StatResponseSkip>();
            FailDict = new Dictionary<string, StatResponseFail>();
        }

        #endregion

        #region Methods

        #region Get Methods

        /// <summary> Retrieves a class result by its PIQI SAM entity type mnemonic. </summary>
        public StatResponseClass GetClass(EvaluationResult evaluationResult)
        {
            string key = $"{evaluationResult.EvaluationItem?.ClassEntityMnemonic}";
            return ClassDict.ContainsKey(key) ? ClassDict[key] : null;
        }

        /// <summary> Retrieves an element result by its evaluation result evaluation item class mnemonic and element sequence. </summary>
        public StatResponseElement GetElement(EvaluationResult evaluationResult)
        {
            string key = $"{evaluationResult.EvaluationItem?.ClassEntityMnemonic}.{evaluationResult.EvaluationItem?.ElementSequence}";
            return ElementDict.ContainsKey(key) ? ElementDict[key] : null;
        }

        /// <summary> Retrieves a critical failure result by PIQI SAM keys. </summary>
        public StatResponseCriticalFailure GetCriticalFailure(EvaluationResult evaluationResult)
        {
            string key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}|{evaluationResult.FailSamMnemonic}";
            return CritcalFailureDict.ContainsKey(key) ? CritcalFailureDict[key] : null;
        }

        /// <summary> Retrieves an informational result by PIQI SAM keys. </summary>
        public StatResponseInformational GetInformational(EvaluationResult evaluationResult)
        {
            string key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}";
            return InformationalDict.ContainsKey(key) ? InformationalDict[key] : null;
        }

        /// <summary> Retrieves a skipped SAM result by PIQI SAM keys. </summary>
        public StatResponseSkip GetSkip(EvaluationResult evaluationResult)
        {
            string key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}|{evaluationResult.SkipSamMnemonic}";
            return SkipDict.ContainsKey(key) ? SkipDict[key] : null;
        }

        /// <summary> Retrieves a failed SAM result by PIQI SAM keys. </summary>
        public StatResponseFail GetFail(EvaluationResult evaluationResult)
        {
            string key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}|{evaluationResult.FailSamMnemonic}";
            return FailDict.ContainsKey(key) ? FailDict[key] : null;
        }

        #endregion

        #region Increment Methods

        /// <summary>
        /// Increments the total count for the given PIQI SAM. 
        /// Updates scoring totals if it is a scoring SAM; otherwise updates informational totals.
        /// Weighted total is incremented by the scoring weight for scoring SAMs.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public void IncrementTotal(EvaluationResult evaluationResult)
        {
            if (evaluationResult.IsScoring)
            {
                ScoringTotalCount++;
                WeightedTotalCount += evaluationResult.Criterion?.ScoringWeight ?? 0;
            }
            else
            {
                InfoTotalCount++;
            }
        }

        /// <summary>
        /// Increments the skipped count for the given PIQI SAM.
        /// Updates scoring skipped count if it is a scoring SAM; otherwise updates informational skipped count.
        /// Weighted skipped is incremented by the scoring weight for scoring SAMs.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public void IncrementSkipped(EvaluationResult evaluationResult)
        {
            if (evaluationResult.IsScoring)
            {
                ScoringSkipCount++;
                WeightedSkipCount += evaluationResult.Criterion?.ScoringWeight ?? 0;
            }
            else
            {
                InfoSkipCount++;
            }
        }

        /// <summary>
        /// Increments the processed count for the given PIQI SAM.
        /// Updates scoring processed count if it is a scoring SAM; otherwise updates informational processed count.
        /// Weighted processed is incremented by the scoring weight for scoring SAMs.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public void IncrementProcessed(EvaluationResult evaluationResult)
        {
            if (evaluationResult.IsScoring)
            {
                ScoringProcCount++;
                WeightedProcCount += evaluationResult.Criterion?.ScoringWeight ?? 0;
            }
            else
            {
                InfoProcCount++;
            }
        }

        /// <summary>
        /// Increments the passed count for the given PIQI SAM.
        /// Updates scoring passed count if it is a scoring SAM; otherwise updates informational passed count.
        /// Weighted passed is incremented by the scoring weight for scoring SAMs.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public void IncrementPassed(EvaluationResult evaluationResult)
        {
            if (evaluationResult.IsScoring)
            {
                ScoringPassCount++;
                WeightedPassCount += evaluationResult.Criterion?.ScoringWeight ?? 0;
            }
            else
            {
                InfoPassCount++;
            }
        }

        /// <summary>
        /// Increments the failed count for the given PIQI SAM.
        /// Updates scoring failed count if it is a scoring SAM; otherwise updates informational failed count.
        /// Weighted failed is incremented by the scoring weight for scoring SAMs.
        /// If the SAM is critical, also increments the critical failure count.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public void IncrementFailed(EvaluationResult evaluationResult)
        {
            if (evaluationResult.IsScoring)
            {
                ScoringFailCount++;
                WeightedFailCount += evaluationResult.Criterion?.ScoringWeight ?? 0;
                if (evaluationResult.IsCritical)
                {
                    CriticalFailureCount++;
                }
            }
            else
            {
                InfoFailCount++;
            }
        }

        #endregion


        /// <summary>
        /// Processes a single PIQI SAM result and updates the corresponding scoring and informational statistics
        /// in this <see cref="StatResponse"/> instance.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        /// <param name="referenceData">The reference data used for evaluation (not used in current logic, but available for future use).</param>
        /// <remarks>
        /// - Conditional and dependent PIQI SAMs are ignored.
        /// - Updates totals, processed, skipped, passed, failed, and weighted counts.
        /// - Logs scoring, skipped, fail, critical failure, and informational SAMs in the corresponding dictionaries.
        /// </remarks>
        public void ProcessResult(EvaluationResult evaluationResult, PIQIReferenceData referenceData)
        {
            // Ignore conditional and dependent PIQI SAMs
            if (evaluationResult.IsConditional || evaluationResult.IsDependent) return;

            // Increment the total if the PIQI SAM is scoring
            IncrementTotal(evaluationResult);

            // If the failure is informational, log it in the informationalDict
            if (!evaluationResult.IsScoring)
            {
                StatResponseInformational informational = GetInformational(evaluationResult);
                if (informational == null)
                {
                    informational = new StatResponseInformational(evaluationResult);
                    InformationalDict.Add(informational.Key, informational);
                }
                informational.Increment(evaluationResult.EvalResult);
            }

            // Log class records for all scoring PIQI SAMs. This is used for auditing.
            StatResponseElement piqiElement = GetElement(evaluationResult);
            if (piqiElement == null)
            {
                piqiElement = new StatResponseElement(evaluationResult);
                ElementDict.Add(piqiElement.Key, piqiElement);
            }
            piqiElement.Increment(evaluationResult);

            // Check if the PIQI SAM was skipped
            if (evaluationResult.EvalSkipped)
            {
                IncrementSkipped(evaluationResult);

                StatResponseSkip skip = GetSkip(evaluationResult);
                if (skip == null)
                {
                    skip = new StatResponseSkip(evaluationResult);
                    SkipDict.Add(skip.Key, skip);
                }
                skip.SkipCount++;
            }
            else
            {
                // Processed (passed or failed)
                IncrementProcessed(evaluationResult);

                if (evaluationResult.EvalPassed)
                {
                    IncrementPassed(evaluationResult);
                }
                else
                {
                    IncrementFailed(evaluationResult);

                    StatResponseFail fail = GetFail(evaluationResult);
                    if (fail == null)
                    {
                        fail = new StatResponseFail(evaluationResult);
                        FailDict.Add(fail.Key, fail);
                    }
                    fail.FailCount++;

                    if (evaluationResult.IsCritical)
                    {
                        StatResponseCriticalFailure criticalFailure = GetCriticalFailure(evaluationResult);
                        if (criticalFailure == null)
                        {
                            criticalFailure = new StatResponseCriticalFailure(evaluationResult);
                            CritcalFailureDict.Add(criticalFailure.Key, criticalFailure);
                        }
                        criticalFailure.Increment(evaluationResult.EvalResult);
                    }
                }
            }
        }
        #endregion
    }
}
