namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the result of a critical failure for a specific PIQI SAM.
    /// Tracks counts of total, skipped, processed, passed, and failed executions.
    /// </summary>
    public class StatResponseCriticalFailure
    {
        #region Properties

        /// <summary>
        /// The mnemonic of the entity associated with this critical failure.
        /// </summary>
        public string EntityMnemonic { get; set; }

        /// <summary>
        /// The mnemonic of the SAM associated with this critical failure.
        /// </summary>
        public string SAMMnemonic { get; set; }

        /// <summary>
        /// Unique key for this critical failure, typically combining entity and SAM mnemonics.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Indicates whether this SAM contributes to scoring.
        /// </summary>
        public bool IsScoring { get; set; }

        /// <summary>
        /// Indicates whether this SAM is a critical check.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// The scoring weight of this SAM.
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Total number of SAMs executed for this critical failure.
        /// </summary>
        public int SAMTotalCount { get; set; }

        /// <summary>
        /// Number of skipped SAM executions.
        /// </summary>
        public int SAMSkippedCount { get; set; }

        /// <summary>
        /// Number of processed SAM executions (excluding skipped).
        /// </summary>
        public int SAMProcessedCount { get; set; }

        /// <summary>
        /// Number of processed SAMs that passed.
        /// </summary>
        public int SAMPassedCount { get; set; }

        /// <summary>
        /// Number of processed SAMs that failed.
        /// </summary>
        public int SAMFailedCount { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatResponseCriticalFailure"/> class.
        /// Default constructor.
        /// </summary>
        public StatResponseCriticalFailure() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatResponseCriticalFailure"/> class
        /// based on a <see cref="PIQISAM"/> object.
        /// </summary>
        /// <param name="piqiSam">The PIQI SAM object to create the critical failure from.</param>
        public StatResponseCriticalFailure(EvaluationResult evaluationResult)
        {
            EntityMnemonic = evaluationResult.EntityMnemonic;
            SAMMnemonic = evaluationResult.SamMnemonic;
            Key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}|{evaluationResult.FailSamMnemonic}";
            IsCritical = evaluationResult.IsCritical;
            IsScoring = evaluationResult.IsScoring;
            Weight = evaluationResult.Criterion?.ScoringWeight ?? 1;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Increments the counts for this critical failure based on the SAM processing state.
        /// </summary>
        /// <param name="state">The processing state of the SAM (Skipped, Passed, Failed).</param>
        public void Increment(ProcessStateEnum state)
        {
            SAMTotalCount++;

            if (state == ProcessStateEnum.Skipped)
            {
                SAMSkippedCount++;
            }
            else
            {
                SAMProcessedCount++;
                if (state == ProcessStateEnum.Passed)
                    SAMPassedCount++;
                else
                    SAMFailedCount++;
            }
        }

        #endregion
    }
}
