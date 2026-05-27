namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a failed PIQI SAM execution for an element.
    /// Tracks the entity, SAM identifiers, whether it is scoring, critical, and the fail count.
    /// </summary>
    public class StatResponseFail
    {
        #region Properties

        /// <summary>
        /// The mnemonic of the entity associated with the failed SAM.
        /// </summary>
        public string EntityMnemonic { get; set; }

        /// <summary>
        /// The mnemonic of the SAM that was executed.
        /// </summary>
        public string SAMMnemonic { get; set; }

        /// <summary>
        /// The mnemonic of the SAM that caused the failure.
        /// </summary>
        public string FailSAMMnemonic { get; set; }

        /// <summary>
        /// Unique key combining entity, SAM, and failure SAM identifiers.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Indicates if the failed SAM is scoring.
        /// </summary>
        public bool IsScoring { get; set; }

        /// <summary>
        /// Indicates if the failure is considered critical.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// The total number of failures recorded for this SAM.
        /// </summary>
        public int FailCount { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="StatResponseFail"/> with the specified parameters.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public StatResponseFail(EvaluationResult evaluationResult)
        {
            EntityMnemonic = evaluationResult.EntityMnemonic;
            SAMMnemonic = evaluationResult.SamMnemonic;
            FailSAMMnemonic = evaluationResult.FailSamMnemonic;
            Key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}|{evaluationResult.FailSamMnemonic}";
            IsScoring = evaluationResult.IsScoring;
            IsCritical = evaluationResult.IsCritical;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StatResponseFail"/> with the specified parameters.
        /// </summary>
        /// <param name="entityMnemonic">The entity mnemonic.</param>
        /// <param name="samMnemonic">The SAM mnemonic.</param>
        /// <param name="failSamMnemonic">The mnemonic of the failing SAM.</param>
        /// <param name="isScoring">Indicates if the SAM is scoring.</param>
        /// <param name="isCritical">Indicates if the failure is critical.</param>
        public StatResponseFail(string entityMnemonic, string samMnemonic, string failSamMnemonic, bool isScoring, bool isCritical)
        {
            EntityMnemonic = entityMnemonic;
            SAMMnemonic = samMnemonic;
            FailSAMMnemonic = failSamMnemonic;
            Key = $"{entityMnemonic}|{samMnemonic}|{failSamMnemonic}";
            IsScoring = isScoring;
            IsCritical = isCritical;
        }

        #endregion
    }
}
