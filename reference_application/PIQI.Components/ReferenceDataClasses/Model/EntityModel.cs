namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a hierarchical model of entities, with a root and children.
    /// </summary>
    public class EntityModel
    {
        #region Properties

        /// <summary>
        /// The root entity in the hierarchy (entity with no parent).
        /// </summary>
        public Entity Root { get; set; }

        /// <summary>
        /// Flattened list of all entities in the model, in hierarchical order.
        /// </summary>
        public List<Entity> EntityList { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityModel"/> class
        /// with an empty <see cref="EntityList"/>.
        /// </summary>
        public EntityModel()
        {
            EntityList = new List<Entity>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityModel"/> class
        /// using the specified <see cref="Model"/>.
        /// </summary>
        /// <param name="model">
        /// The <see cref="Model"/> containing root entity information and model data classes
        /// used to build the entity hierarchy.
        /// </param>
        /// <remarks>
        /// This constructor creates the <see cref="Root"/> entity using the root entity
        /// name and mnemonic from <paramref name="model"/>. It then initializes
        /// <see cref="EntityList"/> and builds the hierarchy by processing each
        /// data class in <see cref="model.ModelDataClasses"/>.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if an error occurs during hierarchy construction.
        /// </exception>
        public EntityModel(Model model)
        {
            try
            {
                // Find root entity (entity with no parent)
                Root = new Entity(model.RootEntityName, model.RootEntityMnemonic);
                if (Root == null) return;

                EntityList = new List<Entity>();

                // Build hierarchy starting from root
                foreach (Entity classEntity in model.DataClasses)
                {
                    ProcessEntity(classEntity, Root);
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the entity hierarchy and populates the EntityList.
        /// </summary>
        private void ProcessEntity(Entity parentEntity, Entity root)
        {
            try
            {
                Entity? classEntity = null;
                Entity? elementEntity = null;

                if (parentEntity?.Children != null)
                {
                    foreach (Entity entity in parentEntity.Children.OrderBy(e => e.EntityType.EntityTypeValue))
                    {
                        if (entity.EntityType.EntityTypeValue == EntityDataTypeEnum.CLS)
                        {
                            classEntity = new Entity(entity, parentEntity.Cardinality?.CardinalityValue);
                            if (root != null)
                            {
                                if (root.Children == null) root.Children = new List<Entity>();
                                root.Children.Add(classEntity);
                            }
                            EntityList.Add(classEntity);
                        }
                        else if (entity.EntityType.EntityTypeValue == EntityDataTypeEnum.ELM)
                        {
                            elementEntity = new Entity(entity, null);
                            if (classEntity != null)
                            {
                                if (classEntity.Children == null) classEntity.Children = new List<Entity>();
                                classEntity.Children.Add(elementEntity);
                            }
                            EntityList.Add(elementEntity);
                        }
                        else if (entity.EntityType.EntityTypeValue > EntityDataTypeEnum.ELM)
                        {
                            if (elementEntity != null)
                            {
                                if (elementEntity.Children == null) elementEntity.Children = new List<Entity>();
                                elementEntity.Children.Add(entity);
                            }
                            EntityList.Add(entity);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            
        }
        #endregion
    }
}
