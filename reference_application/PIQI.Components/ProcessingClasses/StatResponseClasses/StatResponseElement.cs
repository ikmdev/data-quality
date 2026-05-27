namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the result statistics for a specific PIQI SAM element.
    /// Tracks counts of total, skipped, processed, passed, failed, and critical failures.
    /// </summary>
    public class StatResponseElement
    {
        #region Properties

        /// <summary>
        /// The entity type mnemonic associated with this element.
        /// </summary>
        public string? ClassMnemonic { get; set; }

        /// <summary>
        /// The sequence number of this element within its entity type.
        /// </summary>
        public int? Sequence { get; set; }

        /// <summary>
        /// Unique key for this element, typically combining entity type mnemonic and sequence.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Total number of SAMs executed for this element.
        /// </summary>
        public int SAMTotalCount { get; set; }

        /// <summary>
        /// Number of skipped SAM executions.
        /// </summary>
        public int SAMSkipCount { get; set; }

        /// <summary>
        /// Number of processed SAM executions (excluding skipped).
        /// </summary>
        public int SAMProcessedCount { get; set; }

        /// <summary>
        /// Number of scoring SAMs processed for this element.
        /// </summary>
        public int SAMScoringProcessedCount { get; set; }

        /// <summary>
        /// Number of informational SAMs processed for this element.
        /// </summary>
        public int SAMInfoProcessedCount { get; set; }

        /// <summary>
        /// Number of scoring SAMs that passed.
        /// </summary>
        public int SAMPassCount { get; set; }

        /// <summary>
        /// Number of scoring SAMs that failed.
        /// </summary>
        public int SAMFailCount { get; set; }

        /// <summary>
        /// Total weighted denominator of scoring SAMs for this element.
        /// </summary>
        public int SAMWeightedDenominator { get; set; }

        /// <summary>
        /// Total weighted numerator (passed weight) of scoring SAMs for this element.
        /// </summary>
        public int SAMWeightedNumerator { get; set; }

        /// <summary>
        /// Number of critical failures for this element.
        /// </summary>
        public int SAMCriticalFailureCount { get; set; }

        /// <summary>
        /// Indicates if the element is clean (no failed SAMs).
        /// </summary>
        public bool IsClean { get { return SAMFailCount < 1; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StatResponseElement() { }

        /// <summary>
        /// Initializes a new instance of <see cref="StatResponseElement"/> with specified entity type and sequence.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public StatResponseElement(EvaluationResult evaluationResult)
        {
            Sequence = evaluationResult.EvaluationItem?.ElementSequence;
            ClassMnemonic = evaluationResult.EvaluationItem?.ClassEntityMnemonic;
            Key = $"{evaluationResult.EvaluationItem?.ClassEntityMnemonic}.{evaluationResult.EvaluationItem?.ElementSequence}";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Increments the statistics of the element based on the provided PIQI SAM.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public void Increment(EvaluationResult evaluationResult)
        {
            // Always increment total 
            SAMTotalCount++;

            if (evaluationResult.EvalSkipped)
            {
                // Increment skip count if the state is skipped
                SAMSkipCount++;
            }
            else
            {
                // Increment processed count and weighted total if the state is not skipped
                SAMProcessedCount++;

                if (evaluationResult.IsScoring)
                {
                    SAMScoringProcessedCount++;
                    SAMWeightedDenominator += evaluationResult.Criterion?.ScoringWeight ?? 0;

                    if (evaluationResult.EvalPassed)
                    {
                        // Increment pass count and weighted numerator if passed
                        SAMPassCount++;
                        SAMWeightedNumerator += evaluationResult.Criterion?.ScoringWeight ?? 0;
                    }
                    else
                    {
                        // Increment fail count if failed
                        SAMFailCount++;

                        // Increment critical fail count if the failure is critical
                        if (evaluationResult.IsCritical) SAMCriticalFailureCount++;
                    }
                }
                else
                {
                    // Increment informational processed count if not scoring
                    SAMInfoProcessedCount++;
                }
            }
        }

        #endregion
    }
}
