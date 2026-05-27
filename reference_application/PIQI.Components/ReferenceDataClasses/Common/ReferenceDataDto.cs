namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a reference data item with a label and a value.
    /// </summary>
    public class ReferenceDataDto
    {
        /// <summary>
        /// The display label for the reference data.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The value corresponding to the reference data label.
        /// </summary>
        public string Value { get; set; }
    }
}
