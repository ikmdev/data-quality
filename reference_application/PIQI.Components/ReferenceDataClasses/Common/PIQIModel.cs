namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a PIQI model with basic metadata.
    /// </summary>
    public class PIQIModel
    {
        /// <summary>
        /// The name of the model.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The version number of the model.
        /// </summary>
        public double Version { get; set; }

        /// <summary>
        /// Optional mnemonic identifier for the model.
        /// </summary>
        public string? Mnemonic { get; set; }
    }
}
