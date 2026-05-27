namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the state of a message model header value.
    /// </summary>
    public enum MessageModelHeaderValueStateEnum
    {
        /// <summary>
        /// No state or value has been set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The value is the original value provided.
        /// </summary>
        Original = 1,

        /// <summary>
        /// The value is a default value.
        /// </summary>
        Default = 2,

        /// <summary>
        /// The value has been overridden from its original or default state.
        /// </summary>
        Overridden = 3
    }
}
