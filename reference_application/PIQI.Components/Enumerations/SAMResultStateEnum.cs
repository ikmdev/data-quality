namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the possible result states for a Scoring and Assessment Method (SAM) evaluation.
    /// </summary>
    public enum SAMResultStateEnum
    {
        /// <summary>
        /// The SAM evaluation completed successfully without errors.
        /// </summary>
        SUCCEEDED = 0,

        /// <summary>
        /// The SAM evaluation ran but did not meet the required criteria and failed.
        /// </summary>
        FAILED = 1,

        /// <summary>
        /// The SAM evaluation was intentionally skipped, often due to conditions or dependencies.
        /// </summary>
        SKIPPED = 2,

        /// <summary>
        /// The SAM evaluation encountered an unexpected error and could not complete.
        /// </summary>
        ERRORED = 3
    }
}
