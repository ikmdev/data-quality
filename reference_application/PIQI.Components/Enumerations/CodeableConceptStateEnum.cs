namespace PIQI.Components.Models
{
    /// <summary>
    /// Indicates which items are included in a message's codeable concept entity.
    /// </summary>
    public enum CodeableConceptStateEnum
    {
        /// <summary>
        /// No items are included.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only the text is included.
        /// </summary>
        TextOnly = 1,

        /// <summary>
        /// Only the coded concepts are included.
        /// </summary>
        ConceptsOnly = 2,

        /// <summary>
        /// Both text and coded concepts are included.
        /// </summary>
        Both = 3
    }
}
