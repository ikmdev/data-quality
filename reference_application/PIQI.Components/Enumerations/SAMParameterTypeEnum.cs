namespace PIQI.Components.Models
{
    /// <summary>
    /// Defines the different types of SAM (Scoring and Matching) parameters
    /// that can be used for evaluation or configuration.
    /// </summary>
    public enum SAMParameterTypeEnum
    {
        /// <summary>
        /// A comma-separated value (CSV) list of items.
        /// </summary>
        CSV = 1,

        /// <summary>
        /// A regular expression (Regex) pattern.
        /// </summary>
        REGEX = 2,

        /// <summary>
        /// A single value or item.
        /// </summary>
        SINGLE = 3,

        /// <summary>
        /// A single object with structured data.
        /// </summary>
        OBJECT = 4,

        /// <summary>
        /// A collection of structured objects.
        /// </summary>
        OBJECTS = 5,

        /// <summary>
        /// A collection of structured objects.
        /// </summary>
        VAL_SET = 6,

        /// <summary>
        /// A collection of structured objects.
        /// </summary>
        RNG_SET = 7
    }
}