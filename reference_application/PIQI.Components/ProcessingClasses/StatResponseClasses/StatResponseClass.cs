namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the aggregated results for a class of PIQI SAMs.
    /// Tracks totals, skips, processed counts, passes, fails, and weighted scores for a specific entity type.
    /// </summary>
    public class StatResponseClass
    {
        #region Properties

        /// <summary>
        /// The mnemonic of the entity type this class result represents.
        /// </summary>
        public string ClassMnemonic { get; set; }

        /// <summary>
        /// Unique key for the class result, typically the same as <see cref="ClassMnemonic"/>.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Total number of elements in this class.
        /// </summary>
        public int ElementCount { get; set; }

        /// <summary>
        /// Total number of critical failures in this class.
        /// </summary>
        public int CriticalFailureCount { get; set; }

        /// <summary>
        /// Number of elements that are considered clean (no failures).
        /// </summary>
        public int CleanCount { get; set; }

        /// <summary>
        /// Total number of PIQI SAMs for this class.
        /// </summary>
        public int SAMTotalCount { get; set; }

        /// <summary>
        /// Total number of skipped PIQI SAMs for this class.
        /// </summary>
        public int SAMSkipCount { get; set; }

        /// <summary>
        /// Total number of processed PIQI SAMs for this class.
        /// </summary>
        public int SAMProcessedCount { get; set; }

        /// <summary>
        /// Total number of scoring PIQI SAMs processed.
        /// </summary>
        public int SAMScoringProcessedCount { get; set; }

        /// <summary>
        /// Total number of informational PIQI SAMs processed.
        /// </summary>
        public int SAMInfoProcessedCount { get; set; }

        /// <summary>
        /// Total number of PIQI SAMs that passed.
        /// </summary>
        public int SAMPassCount { get; set; }

        /// <summary>
        /// Total number of PIQI SAMs that failed.
        /// </summary>
        public int SAMFailCount { get; set; }

        /// <summary>
        /// Weighted denominator for scoring calculations.
        /// </summary>
        public int SAMWeightedDenominator { get; set; }

        /// <summary>
        /// Weighted numerator for scoring calculations.
        /// </summary>
        public int SAMWeightedNumerator { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatResponseClass"/> class
        /// for the specified entity type mnemonic.
        /// </summary>
        /// <param name="classMnemonic">The mnemonic of the entity type this class result represents.</param>
        public StatResponseClass(string classMnemonic, int elementCount)
        {
            ClassMnemonic = classMnemonic;
            Key = $"{classMnemonic}";
            ElementCount = elementCount;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the aggregated statistics for this class from a list of element results.
        /// </summary>
        /// <param name="elementList">The list of <see cref="StatResponseElement"/> objects to aggregate.</param>
        public void Calc(List<StatResponseElement> elementList)
        {
            CriticalFailureCount = elementList.Sum(t => t.SAMCriticalFailureCount);
            CleanCount = elementList.Count(t => t.IsClean);

            SAMTotalCount = elementList.Sum(t => t.SAMTotalCount);
            SAMSkipCount = elementList.Sum(t => t.SAMSkipCount);
            SAMProcessedCount = elementList.Sum(t => t.SAMProcessedCount);
            SAMScoringProcessedCount = elementList.Sum(t => t.SAMScoringProcessedCount);
            SAMInfoProcessedCount = elementList.Sum(t => t.SAMInfoProcessedCount);
            SAMPassCount = elementList.Sum(t => t.SAMPassCount);
            SAMFailCount = elementList.Sum(t => t.SAMFailCount);

            SAMWeightedDenominator = elementList.Sum(t => t.SAMWeightedDenominator);
            SAMWeightedNumerator = elementList.Sum(t => t.SAMWeightedNumerator);
        }

        #endregion
    }
}
