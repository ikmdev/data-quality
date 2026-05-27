namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a skipped PIQI SAM result and tracks the skip count.
    /// </summary>
    public class StatResponseSkip
    {
        #region Properties

        /// <summary>
        /// The mnemonic of the entity associated with this skipped SAM.
        /// </summary>
        public string EntityMnemonic { get; set; }

        /// <summary>
        /// The mnemonic of the SAM.
        /// </summary>
        public string SAMMnemonic { get; set; }

        /// <summary>
        /// The mnemonic of the specific skipped SAM instance.
        /// </summary>
        public string SkipSAMMnemonic { get; set; }

        /// <summary>
        /// Unique key combining entity, SAM, and skipped SAM mnemonics.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Indicates whether this SAM is scoring.
        /// </summary>
        public bool IsScoring { get; set; }

        /// <summary>
        /// The number of times this SAM was skipped.
        /// </summary>
        public int SkipCount { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="StatResponseSkip"/>.
        /// </summary>
        /// <param name="evaluationResult">The evaluation result item.</param>
        public StatResponseSkip(EvaluationResult evaluationResult)
        {
            EntityMnemonic = evaluationResult.EntityMnemonic;
            SAMMnemonic = evaluationResult.SamMnemonic;
            SkipSAMMnemonic = evaluationResult.SkipSamMnemonic;
            Key = $"{evaluationResult.EntityMnemonic}|{evaluationResult.SamMnemonic}|{evaluationResult.SkipSamMnemonic}";
            IsScoring = evaluationResult.IsScoring;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StatResponseSkip"/>.
        /// </summary>
        /// <param name="entityMnemonic">The entity mnemonic.</param>
        /// <param name="samMnemonic">The SAM mnemonic.</param>
        /// <param name="skipSamMnemonic">The skipped SAM mnemonic.</param>
        /// <param name="isScoring">Indicates if the SAM is scoring.</param>
        public StatResponseSkip(string entityMnemonic, string samMnemonic, string skipSamMnemonic, bool isScoring)
        {
            EntityMnemonic = entityMnemonic;
            SAMMnemonic = samMnemonic;
            SkipSAMMnemonic = skipSamMnemonic;
            Key = $"{entityMnemonic}|{samMnemonic}|{skipSamMnemonic}";
            IsScoring = isScoring;
        }

        #endregion
    }
}
