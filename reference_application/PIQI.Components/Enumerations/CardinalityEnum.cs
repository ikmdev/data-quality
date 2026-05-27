namespace PIQI.Components.Models
{
    /// <summary>
    /// Specifies the cardinality rules for classes in a model.
    /// </summary>
    public enum CardinalityEnum
    {
        /// <summary>
        /// Exactly one instance is required.
        /// </summary>
        ONE = 1,

        /// <summary>
        /// Zero or more instances are allowed.
        /// </summary>
        ZERO_TO_MANY = 2,

        /// <summary>
        /// One or more instances are required.
        /// </summary>
        ONE_TO_MANY = 3
    }
}
