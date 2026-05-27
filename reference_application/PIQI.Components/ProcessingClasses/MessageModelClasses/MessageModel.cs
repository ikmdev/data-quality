using Newtonsoft.Json.Linq;
using PIQI.Components.Services;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a message model for processing message data against an entity model.
    /// It contains the header, root item, classes, elements, and attributes.
    /// </summary>
    public class MessageModel
    {
        #region Properties

        /// <summary>
        /// The list of <see cref="DataType"/> objects used to look up value types in the message.
        /// </summary>
        public List<DataType> DataTypeList { get; set; } = new List<DataType>();

        /// <summary>
        /// Reference code system to set valid code system in codeable concepts.
        /// </summary>
        public PIQIReferenceData? RefData { get; set; }


        /// <summary>
        /// The <see cref="EntityModel"/> that this message model will be built against.
        /// </summary>
        public EntityModel? EntityModel { get; set; }

        /// <summary>
        /// The root field name the model is build against.
        /// </summary>
        public string? RootEntityName { get; set; }

        /// <summary>
        /// The root mnemonic the model is build against.
        /// </summary>
        public string? RootEntityMnemonic { get; set; }


        /// <summary>
        /// The raw message text to process.
        /// </summary>
        public string? MessageText { get; set; }

        /// <summary>
        /// The header portion of the message.
        /// </summary>
        public MessageModelHeader Header { get; set; } = new MessageModelHeader();

        /// <summary>
        /// Indicates whether the message header has been loaded.
        /// </summary>
        public bool HasHeader { get { return Header != null; } }

        /// <summary>
        /// The root message model item.
        /// </summary>
        public MessageModelItem RootItem { get; set; } = new MessageModelItem();

        /// <summary>
        /// Indicates whether the root item exists.
        /// </summary>
        public bool HasRootItem { get { return RootItem != null; } }

        /// <summary>
        /// Dictionary containing all class items in the message, keyed by "Root|ClassMnemonic".
        /// </summary>
        public Dictionary<string, MessageModelItem> ClassDict { get; set; } = new Dictionary<string, MessageModelItem>();

        /// <summary>
        /// Dictionary containing all element items in the message, keyed by "Root|ClassMnemonic|ElementMnemonic.Sequence".
        /// </summary>
        public Dictionary<string, MessageModelItem> ElementDict { get; set; } = new Dictionary<string, MessageModelItem>();

        /// <summary>
        /// Dictionary containing all attribute items in the message, keyed by "ElementKey|AttributeMnemonic".
        /// </summary>
        public Dictionary<string, MessageModelItem> AttrDict { get; set; } = new Dictionary<string, MessageModelItem>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the dictionaries for classes, elements, and attributes.
        /// </summary>
        private void Initialize()
        {
            ClassDict = new Dictionary<string, MessageModelItem>();
            ElementDict = new Dictionary<string, MessageModelItem>();
            AttrDict = new Dictionary<string, MessageModelItem>();
        }

        #endregion

        #region Methods

        #region Load Methods

        /// <summary>
        /// Loads the header of the message from the provided <see cref="PIQIRequest"/>.
        /// </summary>
        /// <param name="piqiRequest">The PIQI request containing message data and metadata.</param>
        public void LoadHeader(PIQIRequest piqiRequest)
        {
            try
            {
                Initialize();
                MessageText = piqiRequest.MessageData;

                JToken token = JToken.Parse(MessageText);

                Header = new MessageModelHeader(token, piqiRequest.PIQIModelMnemonic, piqiRequest.DataProviderID, piqiRequest.DataSourceID, piqiRequest.MessageID);
                if (Header.ProviderName == null) throw new Exception("Provider is missing.");
                if (Header.DataSourceName == null) throw new Exception("Data Source is missing.");
                if (Header.ClientMessageID == null) throw new Exception("Message ID is missing.");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads the content portion of the message, populating the root, classes, elements, and attributes.
        /// </summary>
        /// <remarks>
        /// This method requires the message header to already be loaded.
        /// The following properties must also be set before calling:
        /// <list type="bullet">
        ///   <item><description><see cref="DataTypeList"/></description></item>
        ///   <item><description><see cref="RootEntityName"/></description></item>
        ///   <item><description><see cref="EntityModel"/></description></item>
        /// </list>
        /// </remarks>
        /// /// <param name="referenceData">The reference data with list of valid code systems.</param>
        public void LoadContent(PIQIReferenceData referenceData)
        {
            try
            {
                if (referenceData != null) RefData = referenceData;
                string? rootName = RootEntityName;
                if (DataTypeList == null) throw new Exception("DataTypeList not initialized");
                if (EntityModel == null) throw new Exception("Entity model not loaded");
                if (rootName == null) throw new Exception("Root not loaded");
                if (MessageText == null) throw new Exception("Message text is missing");
                if (!HasHeader) throw new Exception("Header not loaded");

                JToken token = JToken.Parse(MessageText);
                JToken rootToken = Utility.GetJSONToken(token, rootName);
                if (rootToken == null) throw new Exception("Root token not found");

                RootItem = new MessageModelItem(EntityModel.Root, null, null, EntityModel.Root.Mnemonic, EntityItemTypeEnum.Root);

                ProcessClasses(rootToken);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Process Methods
        // Add class entities to root and class dictionary
        private void ProcessClasses(JToken rootToken)
        {
            // Get the list of classes (elements) for the root
            List<Entity>? classEntityList = EntityModel?.Root.Children;
            if (classEntityList == null) return;

            // Iterate through classes
            foreach (JProperty jProperty in rootToken.Children<JProperty>())
            {
                // Get entity 
                Entity? classEntity = classEntityList.FirstOrDefault(ce => (ce.FieldName?.Equals(jProperty.Name) ?? false) || ce.Name.Equals(jProperty.Name));

                if (classEntity != null)
                {
                    // Create the class item
                    string key = $"{RootItem.Mnemonic}|{classEntity.Mnemonic}";
                    MessageModelItem classItem = new MessageModelItem(classEntity, RootItem, classEntity, key, EntityItemTypeEnum.Class);

                    // Add to root and dict
                    RootItem.AddChildItem(classItem);
                    ClassDict.Add(key, classItem);

                    // Process elements (Classes -> Elements -> Attributes)
                    if (classEntity.Cardinality?.CardinalityValue == CardinalityEnum.ONE)
                        ProcessSingletonElement(classItem, jProperty);
                    else
                        ProcessElements(classItem, jProperty); 
                }
            }
        }

        // Add element entities to the parent class and element dictionary
        private void ProcessElements(MessageModelItem classItem, JProperty propertyToken)
        {
            // Element is the same as the class object
            Entity? elementEntity = classItem?.Entity?.Children?.FirstOrDefault();
            if (elementEntity == null) return;

            // Get the aray of element tokens in the message
            JArray? jArray = propertyToken.Children<JArray>().FirstOrDefault();
            if (jArray == null) return;

            // Maintain sequence number for element key
            int sequence = 1;

            //Iterate through the element tokens
            foreach (JToken jToken in jArray.Children())
            {
                // Create the element item
                string key = $"{RootItem.Mnemonic}|{classItem.Mnemonic}|{elementEntity.Mnemonic}.{sequence}";
                MessageModelItem elementItem = new MessageModelItem(elementEntity, classItem, classItem.Entity, key, EntityItemTypeEnum.Element);

                // Add to class and dictionary
                classItem.AddChildItem(elementItem, $"{elementEntity.Mnemonic}.{sequence}");
                ElementDict.Add(key, elementItem);

                // Process attributes (Elements -> Attributes)
                ProcessAttributes(classItem, elementItem, jToken);

                // Update sequence
                sequence++;
            }
        }

        // Add singleton element entites to parent class and element dictionary (Ex. Demographics)
        private void ProcessSingletonElement(MessageModelItem classItem, JProperty propertyToken)
        {
            // Element is the same as the class object
            Entity? elementEntity = classItem.Entity?.Children?.FirstOrDefault();
            if (elementEntity == null) return;

            // Create the element item
            string key = $"{RootItem.Mnemonic}|{classItem.Mnemonic}|{elementEntity.Mnemonic}.1";
            MessageModelItem elementItem = new MessageModelItem(elementEntity, classItem, classItem.Entity, key, EntityItemTypeEnum.Element);

            // Add to class and dictionary
            classItem.AddChildItem(elementItem, $"{elementEntity.Mnemonic}.1");
            ElementDict.Add(key, elementItem);

            // Process attributes (Elements -> Attributes)
            JObject? jObject = propertyToken.Children<JObject>().FirstOrDefault();
            if (jObject != null)
                ProcessAttributes(classItem, elementItem, jObject);
        }

        // Add attribute entities to parent element and attribute dictionary
        private void ProcessAttributes(MessageModelItem classItem, MessageModelItem elementItem, JToken jToken)
        {
            MessageModelItem? attributeItem = null;
            // Get all children of the passed in element
            List<Entity>? attributeEntityList = elementItem.Entity?.Children;

            if (jToken.Type == JTokenType.Object)
            {
                foreach (JProperty jProperty in ((JObject)jToken).Properties())      // Properties of the element
                {
                    Entity? attributeEntity = attributeEntityList?.FirstOrDefault(ae => string.Equals(ae.FieldName, jProperty.Name, StringComparison.OrdinalIgnoreCase));
                    if (attributeEntity != null)
                    {
                        // Create the attribute item
                        string key = $"{elementItem.Key}|{attributeEntity.Mnemonic}";
                        attributeItem = new MessageModelItem(attributeEntity, elementItem, classItem.Entity, key, EntityItemTypeEnum.Attribute);

                        // Evaluate the attr type
                        if (attributeEntity.EntityType.EntityTypeValue == EntityDataTypeEnum.CC)
                        {
                            if (jProperty.Children<JObject>().Count() > 0)
                            {
                                // This is a complex attribute
                                JObject jObject = jProperty.Children<JObject>().First();
                                attributeItem.MessageText = jObject.ToString();
                                attributeItem.MessageData = new CodeableConcept(jObject, RefData);
                            }
                            else
                            {
                                attributeItem.MessageText = jProperty.Value.ToString();
                                attributeItem.MessageData = new CodeableConcept(jProperty, RefData);
                            }
                        }
                        else if (attributeEntity.EntityType.EntityTypeValue == EntityDataTypeEnum.OBSVAL)
                        {
                            if (jProperty.Children<JObject>().Count() > 0)
                            {
                                // This is a complex attribute
                                JObject jObject = jProperty.Children<JObject>().First();
                                attributeItem.MessageText = jObject.ToString();
                                attributeItem.MessageData = new Value(jObject, DataTypeList, RefData);
                            }
                            else
                            {
                                attributeItem.MessageText = jProperty.Value.ToString();
                                attributeItem.MessageData = new Value(jProperty, DataTypeList, RefData);
                            }
                        }
                        else if (attributeEntity.EntityType.EntityTypeValue == EntityDataTypeEnum.RV)
                        {
                            if (jProperty.Children<JObject>().Count() > 0)
                            {
                                // This is a complex attribute
                                JObject jObject = jProperty.Children<JObject>().First();
                                attributeItem.MessageText = jObject.ToString();
                                attributeItem.MessageData = new ReferenceRange(jObject);
                            }
                            else
                            {
                                attributeItem.MessageText = jProperty.Value.ToString();
                                attributeItem.MessageData = new ReferenceRange(jProperty);
                            }
                        }
                        else
                        {
                            if (jProperty.Children<JObject>().Count() > 0)
                            {
                                // This is a complex attribute
                                JObject jObject = jProperty.Children<JObject>().First();
                                attributeItem.MessageText = jObject.ToString();
                                attributeItem.MessageData = new BaseText(jObject.ToString());
                            }
                            else
                            {
                                attributeItem.MessageText = jProperty.Value.ToString();
                                attributeItem.MessageData = new BaseText(jProperty.Value.ToString());
                            }
                        }

                        // Add to root and dict
                        if (!AttrDict.ContainsKey(key))
                        {
                            elementItem.AddChildItem(attributeItem);
                            AttrDict.Add(key, attributeItem);
                        }
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
