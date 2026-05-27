using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a parameter used by a SAM (Scoring and Analysis Module).
    /// </summary>
    public class SAMParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; } = null!;
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// The value type of the parameter.
        /// </summary>
        public string ParameterValueTypeName { get; set; }

        /// <summary>
        /// Whether or not the sam parameter is optional
        /// </summary>
        public bool IsOptional { get; set; }
    }
}
