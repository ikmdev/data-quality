namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the type of a message item entity.
    /// </summary>
    public enum EntityItemTypeEnum
    {
        /// <summary>
        /// Represents a root-level entity item.
        /// </summary>
        Root = 1,

        /// <summary>
        /// Represents a class-level entity item.
        /// </summary>
        Class = 2,

        /// <summary>
        /// Represents an element-level entity item.
        /// </summary>
        Element = 3,

        /// <summary>
        /// Represents an attribute-level entity item.
        /// </summary>
        Attribute = 4
    }
}
