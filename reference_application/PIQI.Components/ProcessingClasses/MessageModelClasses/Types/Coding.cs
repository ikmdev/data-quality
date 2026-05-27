using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIQI.Components.Services;
using System.Text.Json;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a coding within a codeable concept, including code system, code value, display text, and processing metadata.
    /// </summary>
    public class Coding
    {
        #region Properties

        /// <summary>
        /// List of possible code systems for this coding. A single coding can belong to multiple code systems.
        /// </summary>
        [JsonIgnore]
        public List<string> CodeSystemList { get; set; }

        /// <summary>
        /// The primary code system for this coding.
        /// </summary>
        [JsonProperty(PropertyName = "system")]
        public string CodeSystem { get; set; }

        /// <summary>
        /// The actual code value within the code system.
        /// </summary>
        [JsonProperty(PropertyName = "code")]
        public string CodeValue { get; set; }

        /// <summary>
        /// The display text associated with the code.
        /// </summary>
        [JsonProperty(PropertyName = "display")]
        public string CodeText { get; set; }

        /// <summary>
        /// Recognized code system identified during processing.
        /// </summary>
        [JsonIgnore]
        public string? RecognizedCodeSystem { get; set; }

        /// <summary>
        /// Indicates whether a code system is present.
        /// </summary>
        [JsonIgnore]
        public bool HasCodeSystem { get { return (!string.IsNullOrWhiteSpace(CodeSystem)); } }

        /// <summary>
        /// Indicates whether a code value is present.
        /// </summary>
        [JsonIgnore]
        public bool HasCodeValue { get { return (!string.IsNullOrWhiteSpace(CodeValue)); } }

        /// <summary>
        /// Indicates whether display text is present.
        /// </summary>
        [JsonIgnore]
        public bool HasCodeText { get { return (!string.IsNullOrWhiteSpace(CodeText)); } }

        /// <summary>
        /// Indicates whether the coding has all required components: code system, code value, and code text.
        /// </summary>
        [JsonIgnore]
        public bool IsComplete { get { return (HasCodeSystem && HasCodeValue && HasCodeText); } }

        /// <summary>
        /// Indicates whether the coding has a recognized code system after processing.
        /// </summary>
        [JsonIgnore]
        public bool HasRecognizedCodeSystem { get; set; }

        /// <summary>
        /// Indicates whether the coding is valid.
        /// </summary>
        [JsonIgnore]
        public bool IsValid { get; set; }

        /// <summary>
        /// Indicates whether the coding is active.
        /// </summary>
        [JsonIgnore]
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether the coding is interoperable.
        /// </summary>
        [JsonIgnore]
        public bool IsInteroperable { get; set; }

        /// <summary>
        /// Indicates whether the coding is semantically correct.
        /// </summary>
        [JsonIgnore]
        public bool IsSemantic { get; set; }

        /// <summary>
        /// List of possible display names or aliases for the coding given its code and code system.
        /// </summary>
        [JsonIgnore]
        public List<string> ReferenceDisplayList { get; set; }

        /// <summary>
        /// Stores the response from a FHIR terminology server $lookup operation.
        /// </summary>
        [JsonIgnore]
        public HttpResponseMessage? LookupResponse { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Coding"/> class from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="jToken">The JSON token representing the coding.</param>
        public Coding(JToken jToken)
        {
            ReferenceDisplayList = new List<string>();
            CodeSystemList = Utility.GetJSONStringList(jToken, "system", "system-id");
            CodeValue = Utility.GetJSONString(jToken, "code");
            CodeText = Utility.GetJSONString(jToken, "display");

            if (CodeSystemList != null)
                foreach (string codeSystem in CodeSystemList)
                    if (CodeSystem == null && !string.IsNullOrWhiteSpace(codeSystem))
                        CodeSystem = codeSystem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Coding"/> class from a <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="jElement">The JSON element representing the coding.</param>
        public Coding(JsonElement jElement)
        {
            ReferenceDisplayList = new List<string>();

            if (jElement.TryGetProperty("code", out JsonElement code))
            {
                CodeValue = code.GetString();
            }
            if (jElement.TryGetProperty("system", out JsonElement system))
            {
                CodeSystem = system.GetString();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses the FHIR $lookup response and sets the <see cref="IsActive"/> status based on the coding's status property.
        /// </summary>
        public async void SetStatus()
        {
            try
            {
                if (LookupResponse != null)
                {
                    var content = await LookupResponse.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(content);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("parameter", out JsonElement parameters) && parameters.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement parameter in parameters.EnumerateArray())
                        {
                            if (parameter.TryGetProperty("name", out JsonElement nameElement) &&
                                nameElement.GetString() == "property")
                            {
                                if (parameter.TryGetProperty("part", out JsonElement parts) && parts.ValueKind == JsonValueKind.Array)
                                {
                                    string? code = null;
                                    string? valueCode = null;
                                    bool? valueBool = null;

                                    foreach (JsonElement part in parts.EnumerateArray())
                                    {
                                        if (part.TryGetProperty("name", out JsonElement partName))
                                        {
                                            if (partName.GetString() == "code" &&
                                                part.TryGetProperty("valueCode", out JsonElement codeValue))
                                            {
                                                code = codeValue.GetString();
                                            }
                                            else if (partName.GetString() == "value" &&
                                                     part.TryGetProperty("valueCode", out JsonElement valueCodeElement))
                                            {
                                                valueCode = valueCodeElement.GetString();
                                            }
                                            else if (partName.GetString() == "value" &&
                                                     part.TryGetProperty("valueBoolean", out JsonElement valueBooleanElement))
                                            {
                                                valueBool = valueBooleanElement.GetBoolean();
                                            }
                                        }
                                    }

                                    // Check for Inactive AND Status
                                    if (code == "inactive" && valueBool is bool)
                                    {
                                        IsActive = !(bool)valueBool;
                                        break;
                                    }

                                    if (code == "Status" && valueCode != null)
                                    {
                                        IsActive = valueCode.ToUpper() == "ACTIVE";
                                        break;
                                    } 
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Swallow exceptions for async helper
            }
        }

        /// <summary>
        /// Parses the FHIR $lookup response and populates the <see cref="ReferenceDisplayList"/> with possible display names for the coding.
        /// </summary>
        public async void SetReferenceDisplayList()
        {
            try
            {
                if (LookupResponse != null)
                {
                    var content = await LookupResponse.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(content);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("parameter", out JsonElement parameters) && parameters.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement parameter in parameters.EnumerateArray())
                        {
                            if (parameter.TryGetProperty("valueString", out JsonElement valueStr))
                            {
                                ReferenceDisplayList.Add(valueStr.GetString());
                            }

                            if (parameter.TryGetProperty("name", out JsonElement nameElement) &&
                                nameElement.GetString() == "designation")
                            {
                                if (parameter.TryGetProperty("part", out JsonElement parts) && parts.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (JsonElement part in parts.EnumerateArray())
                                    {
                                        if (part.TryGetProperty("name", out JsonElement partName) &&
                                            partName.GetString() == "value" &&
                                            part.TryGetProperty("valueString", out JsonElement partValueStr))
                                        {
                                            ReferenceDisplayList.Add(partValueStr.GetString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Swallow exceptions for async helper
            }
        }
        #endregion
    }
}
