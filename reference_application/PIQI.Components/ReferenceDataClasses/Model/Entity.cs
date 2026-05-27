using Newtonsoft.Json;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a metadata entity in the system.
    /// </summary>
    public class Entity
    {
        #region Properties
        /// <summary>
        /// Human-readable name of the entity.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Unique mnemonic identifier for the entity.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// Optional short name for the entity.
        /// </summary>
        public string? ShortName { get; set; }

        /// <summary>
        /// The field name in the message corresponding to this entity.
        /// </summary>
        public string? FieldName { get; set; }

        /// <summary>
        /// Optional description of the entity.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Data type ID for this entity.
        /// </summary>
        [JsonProperty(PropertyName = "attributeType")]
        public EntityType? EntityType { get; set; }

        /// <summary>
        /// Cardinality ID for this entity.
        /// </summary>
        public Cardinality? Cardinality { get; set; }

        /// <summary>
        /// Child entities of this entity.
        /// </summary>
        [JsonProperty(PropertyName = "attributes")]
        public List<Entity>? Children { get; set; }

        /// <summary>
        /// Roles of this entity, only applies to data classes.
        /// </summary>
        public List<Role>? Roles { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        public Entity() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> for root 
        /// with the specified name and mnemonic.
        /// </summary>
        /// <param name="name">
        /// The name of the entity. This value is also assigned to 
        /// <see cref="ShortName"/>.
        /// </param>
        /// /// <param name="fieldName">
        /// The json field name for the entity.
        /// </param>
        /// <param name="mnemonic">
        /// The mnemonic identifier for the entity.
        /// </param>
        public Entity(string name, string mnemonic)
        {
            Name = ShortName = FieldName = name;
            Mnemonic = mnemonic;
            EntityType = new EntityType(EntityDataTypeEnum.ROOT);

            Children = new List<Entity>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class using an existing
        /// <see cref="Entity"/> as a base and assigning a new <see cref="CardinalityEnum"/>.
        /// </summary>
        /// <param name="entityBase">
        /// The base <see cref="Entity"/> whose values are copied into the new instance.
        /// Properties such as <c>Name</c>, <c>Mnemonic</c>, <c>ShortName</c>, <c>FieldName</c>,
        /// <c>Description</c>, and <c>DataTypeID</c> are initialized from this object.
        /// </param>
        /// <param name="cardinality">
        /// The <see cref="CardinalityEnum"/> value that specifies the cardinatliy for the new class entity.
        /// </param>
        public Entity(Entity entityBase, CardinalityEnum? cardinality)
        {
            Name = entityBase.Name;
            Mnemonic = entityBase.Mnemonic;
            ShortName = entityBase.ShortName;
            FieldName = entityBase.FieldName;
            Description = entityBase.Description;
            EntityType = entityBase.EntityType;
            if (cardinality.HasValue)
                Cardinality = new Cardinality(cardinality.Value);

            Children = new List<Entity>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a shallow copy of the current entity.
        /// </summary>
        /// <returns>A shallow copy of the entity.</returns>
        public Entity ShallowCopy()
        {
            return (Entity)MemberwiseClone();
        }

        #endregion
    }
}
