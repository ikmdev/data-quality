namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the data type of information stored in a message entity item.
    /// </summary>
    public enum EntityDataTypeEnum
    {
        /// <summary>
        /// Represents a root entity type.
        /// </summary>
        ROOT = 1,

        /// <summary>
        /// Represents a class entity type.
        /// </summary>
        CLS = 2,

        /// <summary>
        /// Represents an element entity type.
        /// </summary>
        ELM = 3,

        /// <summary>
        /// Represents a reference range value entity type.
        /// </summary>
        RV = 4,

        /// <summary>
        /// Represents an observation value entity type.
        /// </summary>
        OBSVAL = 5,

        /// <summary>
        /// Represents a codeable concept entity type.
        /// </summary>
        CC = 6,

        /// <summary>
        /// Represents a text entity type.
        /// </summary>
        ATR = 7
    }
}
