namespace PIQI.Components.Models
{
    /// <summary>
    /// Manages the hierarchical structure of evaluation items and stores all evaluation results
    /// within a single evaluation session.
    /// </summary>
    public class EvaluationManager
    {
        #region Properties

        /// <summary>
        /// Gets or sets the root evaluation item, which serves as the top-level node
        /// for all hierarchical evaluation items.
        /// </summary>
        public EvaluationItem RootItem { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of all evaluation items, keyed by their unique key.
        /// </summary>
        public Dictionary<string, EvaluationItem> EvaluationItemDict { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of all evaluation results, keyed by a unique GUID for each result.
        /// </summary>
        public Dictionary<Guid, EvaluationResult> EvaluationResultDict { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationManager"/> class,
        /// with empty dictionaries for evaluation items and evaluation results.
        /// </summary>
        public EvaluationManager()
        {
            EvaluationItemDict = new Dictionary<string, EvaluationItem>();
            EvaluationResultDict = new Dictionary<Guid, EvaluationResult>();
        }

        #endregion

        #region Load Methods

        /// <summary>
        /// Loads the evaluation hierarchy based on the provided entity model and message model.
        /// Creates <see cref="EvaluationItem"/> instances for the root, classes, elements, and attributes,
        /// and populates the internal dictionaries accordingly.
        /// </summary>
        /// <param name="entityModel">The entity model defining the structure of entities for evaluation.</param>
        /// <param name="messageModel">The message model containing the actual message data.</param>
        public void Load(EntityModel entityModel, MessageModel messageModel)
        {
            // Add the root item
            EvaluationItem? rootEvalItem = AddEvalItem(entityModel.Root, null, messageModel.RootItem);
            if (rootEvalItem != null)
                RootItem = rootEvalItem;

            // Process classes
            if (entityModel?.Root?.Children != null)
            {
                foreach (Entity classEntity in entityModel.Root.Children.OrderBy(t => t.Name))
                {
                    // Get the corresponding message item
                    MessageModelItem? classMessageItem = messageModel.RootItem.ChildDict.TryGetValue(classEntity.Mnemonic, out MessageModelItem? classValue) ? classValue : null;

                    // Add class item
                    EvaluationItem? classEvalItem = AddEvalItem(classEntity, RootItem, classMessageItem);

                    // Process elements
                    // Note: elements only exist within the message model so we process from there
                    if (classMessageItem != null)
                    {
                        Entity? elementEntity = classEntity.Children?.First();
                        foreach (MessageModelItem elementMessageItem in classMessageItem.ChildDict.Values.OrderBy(t => t.ElementSequence))
                        {
                            // Add element item
                            EvaluationItem? elementEvalItem = AddEvalItem(elementEntity, classEvalItem, elementMessageItem);

                            // Process attributes
                            // Note: Attributes come from them entity model where they are direct descendants of the class
                            foreach (Entity attrEntity in elementEntity.Children.OrderBy(t => t.Name))
                            {
                                MessageModelItem? attrMessageItem = elementMessageItem.ChildDict.TryGetValue(attrEntity.Mnemonic, out MessageModelItem? attrValue) ? attrValue : null;
                                // Add attr item
                                EvaluationItem? attrEvalItem = AddEvalItem(attrEntity, elementEvalItem, attrMessageItem);
                            }

                        }
                    }
                }
            }
        }
        #endregion

        #region Item Methods

        /// <summary>
        /// Creates a new <see cref="EvaluationItem"/> for the specified entity and optionally links it to a parent evaluation item
        /// and a message model item. Adds the new item to the parent's child dictionary and the manager's main dictionary.
        /// </summary>
        /// <param name="entity">The entity to create an evaluation item for.</param>
        /// <param name="parentEvaluationItem">The parent evaluation item, if any.</param>
        /// <param name="messageItem">The message model item associated with this evaluation item, if any.</param>
        /// <returns>The newly created <see cref="EvaluationItem"/>.</returns>
        public EvaluationItem? AddEvalItem(Entity entity, EvaluationItem? parentEvaluationItem, MessageModelItem? messageItem = null)
        {
            // Create the eval item
            EvaluationItem newEvaluationItem = new EvaluationItem(entity, parentEvaluationItem, messageItem);

            // Add the eval item to various dicts
            if (parentEvaluationItem != null)
                parentEvaluationItem.AddChildItem(newEvaluationItem);            // Add to parent via local key
            EvaluationItemDict.Add(newEvaluationItem.Key, newEvaluationItem);     // Add to main dict via full key

            // If it's the root, special handling
            if (newEvaluationItem.ItemType == EntityItemTypeEnum.Root) RootItem = newEvaluationItem;

            // Return our new item
            return newEvaluationItem;
        }
        #endregion

        #region Result Methods

        /// <summary>
        /// Retrieves an <see cref="EvaluationResult"/> from the manager's dictionary using its unique identifier.
        /// </summary>
        /// <param name="resultUID">The unique identifier (GUID) of the evaluation result to retrieve.</param>
        /// <returns>
        /// The <see cref="EvaluationResult"/> corresponding to the specified <paramref name="resultUID"/>.
        /// Returns <c>null</c> if no matching result is found.
        /// </returns>
        public EvaluationResult GetEvalResult(Guid resultUID)
        {
            if (EvaluationResultDict.ContainsKey(resultUID))
                return EvaluationResultDict[resultUID];
            return null;
        }

        /// <summary>
        /// Creates a new <see cref="EvaluationResult"/> for a given evaluation item, criterion, and SAM,
        /// adds it to the item's criteria result dictionary, and also adds it to the manager's result dictionary.
        /// </summary>
        /// <param name="evaluationItem">The evaluation item to which the result belongs.</param>
        /// <param name="criterion">The evaluation criterion associated with the result.</param>
        /// <param name="sam">The SAM object used for the evaluation.</param>
        /// <param name="isConditional">
        /// Indicates whether this result is conditional on other results or conditions.
        /// </param>
        /// <param name="isDependent">
        /// Indicates whether this result is dependent on other evaluation results.
        /// </param>
        /// <returns>The newly created <see cref="EvaluationResult"/> instance.</returns>
        public EvaluationResult CreateEvalResult(EvaluationItem evaluationItem, EvaluationCriterion criterion, SAM sam, bool isConditional, bool isDependent)
        {
            EvaluationResult result = evaluationItem.AddCriterionResult(criterion, sam); // Adds to item's CriteriaResultDict
            EvaluationResultDict.Add(result.ResultUID, result);
            return result;
        }
        #endregion
    }
}
