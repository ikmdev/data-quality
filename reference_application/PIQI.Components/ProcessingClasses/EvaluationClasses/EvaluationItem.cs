using System.Text.Json.Serialization;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a single item within an evaluation, including its associated entity, message data,
    /// child evaluation items, and the results of evaluation criteria.
    /// </summary>
    public class EvaluationItem
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique key identifying this evaluation item.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the message model item associated with this evaluation item.
        /// This property is ignored during JSON serialization.
        /// </summary>
        [JsonIgnore]
        public MessageModelItem? MessageItem { get; set; }

        /// <summary>
        /// Gets or sets the entity this evaluation item represents.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic of the root entity for this evaluation item.
        /// </summary>
        public string RootEntityMnemonic { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic of the class entity if this item represents a class or lower.
        /// </summary>
        public string? ClassEntityMnemonic { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic of the element entity if this item represents an element or lower.
        /// </summary>
        public string? ElementEntityMnemonic { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the element within its parent.
        /// </summary>
        public int? ElementSequence { get; set; }

        /// <summary>
        /// Gets the type of this evaluation item based on the entity's type.
        /// </summary>
        public EntityItemTypeEnum? ItemType
        {
            get
            {
                if (Entity.EntityType.EntityTypeValue == null)
                    return null;
                return (EntityItemTypeEnum)Math.Min((int)Entity.EntityType.EntityTypeValue, 4);
            }
        }

        /// <summary>
        /// Gets or sets the dictionary of child evaluation items.
        /// Keys are based on the message item's local key.
        /// </summary>
        public Dictionary<string, EvaluationItem> ChildDict { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of evaluation results for each criterion, keyed by SAM mnemonic.
        /// </summary>
        public Dictionary<string, EvaluationResult> CriteriaResultDict { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of all evaluation results, including conditionals and non-skipped dependents.
        /// </summary>
        public Dictionary<string, EvaluationResult> FullResultDict { get; set; }

        /// <summary>
        /// Gets or sets the group state of this evaluation item.
        /// Values correspond to eGROUP_RESULT_STATE: None, All passed, Some passed, None passed.
        /// </summary>
        public int GroupState { get; set; }

        /// <summary>
        /// Gets a value indicating whether this evaluation item has an associated message item.
        /// </summary>
        public bool HasMessageItem => (MessageItem != null);

        /// <summary>
        /// Gets a value indicating whether this evaluation item has any child items.
        /// </summary>
        public bool HasChildren => ChildDict.Values.Count > 0;

        /// <summary>
        /// Gets a value indicating whether this evaluation item has any criterion results.
        /// </summary>
        public bool HasResults => CriteriaResultDict.Values.Count > 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationItem"/> class with the specified entity,
        /// parent evaluation item, and associated message model item.
        /// </summary>
        /// <param name="entity">The entity this evaluation item represents.</param>
        /// <param name="parentItem">The parent evaluation item, if any.</param>
        /// <param name="messageItem">The associated message model item, if any.</param>
        public EvaluationItem(Entity entity, EvaluationItem? parentItem, MessageModelItem? messageItem)
        {
            Entity = entity;
            MessageItem = messageItem;

            if (messageItem?.ElementSequence != null)
                ElementSequence = messageItem.ElementSequence;
            else if (parentItem?.ElementSequence != null)
                ElementSequence = parentItem.ElementSequence;

            Key = messageItem?.Key ?? $"{parentItem?.Key}|{(ItemType == EntityItemTypeEnum.Element ? $"{entity.Mnemonic}|{ElementSequence}" : entity.Mnemonic)}";

            ChildDict = new Dictionary<string, EvaluationItem>();
            CriteriaResultDict = new Dictionary<string, EvaluationResult>();
            FullResultDict = new Dictionary<string, EvaluationResult>();

            switch (ItemType)
            {
                case EntityItemTypeEnum.Root:
                    RootEntityMnemonic = entity.Mnemonic;
                    break;
                case EntityItemTypeEnum.Class:
                    RootEntityMnemonic = parentItem?.RootEntityMnemonic;
                    ClassEntityMnemonic = entity.Mnemonic;
                    break;
                case EntityItemTypeEnum.Element:
                    RootEntityMnemonic = parentItem?.RootEntityMnemonic;
                    ClassEntityMnemonic = parentItem?.ClassEntityMnemonic;
                    ElementEntityMnemonic = entity.Mnemonic;
                    break;
                case EntityItemTypeEnum.Attribute:
                    RootEntityMnemonic = parentItem?.RootEntityMnemonic;
                    ClassEntityMnemonic = parentItem?.ClassEntityMnemonic;
                    ElementEntityMnemonic = parentItem?.ElementEntityMnemonic;
                    break;
            }
        }

        #endregion

        #region Put/Get Methods

        /// <summary>
        /// Adds a child evaluation item to this item's child dictionary.
        /// </summary>
        /// <param name="childItem">The child evaluation item to add.</param>
        public void AddChildItem(EvaluationItem childItem)
        {
            string key = childItem.ElementSequence != null ? $"{childItem.Entity.Mnemonic}|{childItem.ElementSequence}" : childItem.Entity.Mnemonic;
            ChildDict.Add(key, childItem);
        }

        /// <summary>
        /// Adds a new criterion result for this evaluation item at the time of creation.
        /// </summary>
        /// <param name="criterion">The evaluation criterion to apply.</param>
        /// <param name="sam">The SAM object used for evaluation.</param>
        /// <returns>The created <see cref="EvaluationResult"/>.</returns>
        public EvaluationResult AddCriterionResult(EvaluationCriterion criterion, SAM sam)
        {
            EvaluationResult result = new EvaluationResult(this, criterion, sam, false, false);
            CriteriaResultDict.Add($"{sam.Mnemonic}.{criterion.Sequence}", result);
            return result;
        }

        /// <summary>
        /// Adds an evaluation result to the full result dictionary, including non-skipped dependent results.
        /// </summary>
        /// <param name="result">The evaluation result to add.</param>
        public void AddFullResult(EvaluationResult result)
        {
            if (!result.EvalSkipped)
                FullResultDict.Add($"{result.Sam.Mnemonic}.{result.Criterion.Sequence}", result);
        }

        #endregion
    }
}
