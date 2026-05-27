namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a code-value pair for reference or lookup purposes.
    /// </summary>
    public class CodeList
    {
        /// <summary>
        /// The code or identifier of the data item.
        /// </summary>
        public string DataCode { get; set; } = null!;

        /// <summary>
        /// The human-readable text or description corresponding to the code.
        /// </summary>
        public string DataText { get; set; } = null!;
    }
}
