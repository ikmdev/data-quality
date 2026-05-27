using Newtonsoft.Json;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents an item in a message model, which can be a class, element, or attribute.
    /// </summary>
    public class MessageModelItem
    {
        #region Properties

        /// <summary>
        /// Unique key for this message model item, typically including parent and mnemonic information.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Sequence number for elements within a class. Null for class items.
        /// </summary>
        public int? ElementSequence { get; set; }

        /// <summary>
        /// Arbitrary unique mnemonic for this item.
        /// </summary>
        public string Mnemonic { get; set; }

        /// <summary>
        /// The parent item's mnemonic, or null if this is a root item.
        /// </summary>
        public string? ParentItemMnemonic { get; set; }

        /// <summary>
        /// Display name of the item, derived from the underlying entity object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the message model item (Class, Element, Attribute, or Root).
        /// </summary>
        public EntityItemTypeEnum ItemType { get; set; }

        /// <summary>
        /// The underlying entity's class definition for this item.
        /// </summary>
        public Entity? ClassEntity { get; set; }

        /// <summary>
        /// The underlying entity definition for this item.
        /// </summary>
        public Entity? Entity { get; set; }

        /// <summary>
        /// The parsed message data for this item, as a BaseText or derived type.
        /// </summary>
        public BaseText? MessageData { get; set; }

        /// <summary>
        /// Raw message JSON text corresponding to this item. Ignored in serialization.
        /// </summary>
        [JsonIgnore]
        public string? MessageText { get; set; }

        /// <summary>
        /// Dictionary of child items for this message item, keyed by mnemonic or provided key.
        /// </summary>
        public Dictionary<string, MessageModelItem>? ChildDict { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageModelItem"/> class.
        /// </summary>
        public MessageModelItem()
        {
            ChildDict = new Dictionary<string, MessageModelItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageModelItem"/> class from an entity and parent item.
        /// </summary>
        /// <param name="entity">The entity definition for this item.</param>
        /// <param name="parentItem">The parent message item, or null if root.</param>
        /// <param name="classEntity">The parent message item, or null if root.</param>
        /// <param name="key">The unique key for this item.</param>
        /// <param name="itemType">The type of the message model item.</param>
        public MessageModelItem(Entity entity, MessageModelItem? parentItem, Entity? classEntity, string key, EntityItemTypeEnum itemType)
        {
            ChildDict = new Dictionary<string, MessageModelItem>();

            Key = key;
            Mnemonic = entity.Mnemonic;
            ParentItemMnemonic = parentItem?.Mnemonic;
            Name = entity.FieldName ?? entity.Name;
            ItemType = itemType;
            Entity = entity;
            ClassEntity = classEntity;

            // Set class mnemonic and sequence number if applicable
            switch (itemType)
            {
                case EntityItemTypeEnum.Class:
                    break;
                case EntityItemTypeEnum.Element:
                    ElementSequence = parentItem?.ChildDict?.Count() + 1;
                    break;
                case EntityItemTypeEnum.Attribute:
                    ElementSequence = parentItem?.ElementSequence;
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a child item to this item's child dictionary.
        /// </summary>
        /// <param name="childItem">The child <see cref="MessageModelItem"/> to add.</param>
        /// <param name="key">Optional key for the child; if null, the child's mnemonic is used.</param>
        public void AddChildItem(MessageModelItem childItem, string? key = null)
        {
            if (ChildDict == null) ChildDict = new Dictionary<string, MessageModelItem>();
            ChildDict.Add(key ?? childItem.Mnemonic, childItem);
        }

        #endregion
    }
}
