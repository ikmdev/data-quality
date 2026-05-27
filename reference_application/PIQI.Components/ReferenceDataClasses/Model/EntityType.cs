using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the type of an entity, including an optional name and an enumerated data type.
    /// </summary>
    public class EntityType
    {
        #region Properties

        /// <summary>
        /// Gets or sets the optional descriptive name for this entity type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the enumerated value representing the entity's data type.
        /// Serialized as a string in JSON with the property name "Mnemonic".
        /// </summary>
        [JsonProperty(PropertyName = "Mnemonic")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EntityDataTypeEnum EntityTypeValue { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityType"/> class with the specified entity data type.
        /// </summary>
        /// <param name="entityTypeValue">The <see cref="EntityDataTypeEnum"/> value representing the entity type.</param>
        public EntityType(EntityDataTypeEnum entityTypeValue)
        {
            EntityTypeValue = entityTypeValue;
        }

        #endregion
    }
}
