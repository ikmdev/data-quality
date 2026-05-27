using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the cardinality of an entity, including a name and an enumerated value.
    /// </summary>
    public class Cardinality
    {
        #region Properties

        /// <summary>
        /// Gets or sets the optional descriptive name for this cardinality.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the enumerated value representing the cardinality.
        /// Serialized as a string in JSON with the property name "Mnemonic".
        /// </summary>
        [JsonProperty(PropertyName = "Mnemonic")]
        [JsonConverter(typeof(StringEnumConverter))]
        public CardinalityEnum CardinalityValue { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Cardinality"/> class with the specified cardinality value.
        /// </summary>
        /// <param name="cardinalityValue">The <see cref="CardinalityEnum"/> value to assign.</param>
        public Cardinality(CardinalityEnum cardinalityValue)
        {
            CardinalityValue = cardinalityValue;
        }

        #endregion
    }
}
