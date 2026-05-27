using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a parameter used in evaluation criteria.
    /// </summary>
    public class EvaluationCriteriaParameter
    {
        /// <summary>
        /// The mnemonic of the parameter.
        /// </summary>
        public string SamParameterMnemonic { get; set; }
        /// <summary>
        /// The value of the parameter. Can be null.
        /// </summary>
        public string? ParameterValue { get; set; }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string ParameterValueTypeName { get; set; } = null!;

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string ParameterName { get; set; } = null!;
    }
}
