using Newtonsoft.Json.Linq;
using PIQI.Components.Services;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the header portion of a message, including metadata such as provider, data source, and transaction date.
    /// </summary>
    public class MessageModelHeader
    {
        #region Properties

        /// <summary>
        /// The entity model mnemonic information.
        /// </summary>
        public MessageModelHeaderString EntityModelMnemonicData { get; set; }

        /// <summary>
        /// The provider name information.
        /// </summary>
        public MessageModelHeaderString ProviderNameData { get; set; }

        /// <summary>
        /// The data source name information.
        /// </summary>
        public MessageModelHeaderString DataSourceNameData { get; set; }

        /// <summary>
        /// The client message ID information.
        /// </summary>
        public MessageModelHeaderString ClientMessageIDData { get; set; }

        /// <summary>
        /// The transaction date information.
        /// </summary>
        public MessageModelHeaderDate TransactionDateData { get; set; }

        /// <summary>
        /// Gets the entity model mnemonic value.
        /// </summary>
        public string EntityModelMnemonic => EntityModelMnemonicData.Value;

        /// <summary>
        /// Gets the provider name value.
        /// </summary>
        public string ProviderName => ProviderNameData.Value;

        /// <summary>
        /// Gets the data source name value.
        /// </summary>
        public string DataSourceName => DataSourceNameData.Value;

        /// <summary>
        /// Gets the client message ID value.
        /// </summary>
        public string ClientMessageID => ClientMessageIDData.Value;

        /// <summary>
        /// Gets the transaction date value.
        /// </summary>
        public DateTime? TransactionDate => TransactionDateData.Value;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageModelHeader"/> class with default values.
        /// </summary>
        public MessageModelHeader()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageModelHeader"/> class from a JSON token and optional overrides.
        /// </summary>
        /// <param name="pToken">The JSON token containing header data.</param>
        /// <param name="EntityModelMnemonic">Optional override for entity model mnemonic.</param>
        /// <param name="dataProviderID">Optional override for provider name.</param>
        /// <param name="dataSourceID">Optional override for data source name.</param>
        /// <param name="messageID">Optional override for client message ID.</param>
        public MessageModelHeader(JToken pToken, string? EntityModelMnemonic, string? dataProviderID, string? dataSourceID, string? messageID)
        {
            Initialize();

            EntityModelMnemonicData.OriginalValue = Utility.GetJSONString(pToken, "EntityModel") ?? EntityModelMnemonic;
            ProviderNameData.OriginalValue = dataProviderID ?? Utility.GetJSONString(pToken, "DataProviderID");
            DataSourceNameData.OriginalValue = dataSourceID ?? Utility.GetJSONString(pToken, "DataSourceID");
            ClientMessageIDData.OriginalValue = messageID ?? Utility.GetJSONString(pToken, "MessageID");
            TransactionDateData.OriginalValue = Utility.ObjNullableDateTime(Utility.GetJSONString(pToken, "TransactionDate"));
        }

        /// <summary>
        /// Initializes the header properties with default instances.
        /// </summary>
        private void Initialize()
        {
            EntityModelMnemonicData = new MessageModelHeaderString();
            ProviderNameData = new MessageModelHeaderString();
            DataSourceNameData = new MessageModelHeaderString();
            ClientMessageIDData = new MessageModelHeaderString();
            TransactionDateData = new MessageModelHeaderDate();
        }

        #endregion
    }
}
