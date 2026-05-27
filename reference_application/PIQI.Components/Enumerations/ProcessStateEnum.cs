namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the current state of a SAM (Scoring and Analysis Model) processing evaluation.
    /// </summary>
    public enum ProcessStateEnum
    {
        /// <summary>
        /// The SAM evaluation has not yet been processed.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The SAM evaluation completed successfully.
        /// </summary>
        Passed = 1,

        /// <summary>
        /// The SAM evaluation failed.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// The SAM evaluation was skipped.
        /// </summary>
        Skipped = 3
    }
}
