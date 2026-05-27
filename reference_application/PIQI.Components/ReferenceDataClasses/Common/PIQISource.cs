namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a source of PIQI data.
    /// </summary>
    public class PIQISource
    {
        /// <summary>
        /// The name of the source.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The mnemonic identifier for the source.
        /// </summary>
        public string Mnemonic { get; set; } = null!;
    }
}
