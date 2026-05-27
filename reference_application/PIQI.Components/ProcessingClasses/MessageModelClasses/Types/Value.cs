using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIQI.Components.Services;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a value entity in a message, which may include a coded type, numeric values, and text.
    /// Inherits from <see cref="CodeableConcept"/>.
    /// </summary>
    public class Value : CodeableConcept
    {
        #region Properties

        /// <summary>
        /// The coded type of this value, represented as a <see cref="CodeableConcept"/>.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public CodeableConcept TypeCC { get; set; }

        /// <summary>
        /// The resolved <see cref="DataType"/> corresponding to <see cref="TypeCC"/>.
        /// </summary>
        [JsonIgnore]
        public DataType Type { get; set; }

        /// <summary>
        /// The primary numeric value of this instance, if applicable.
        /// </summary>
        [JsonIgnore]
        public double? ValueNumber { get; set; }

        /// <summary>
        /// The secondary numeric value of this instance, if applicable (used for ranges).
        /// </summary>
        [JsonIgnore]
        public double? ValueNumber2 { get; set; }

        /// <summary>
        /// Stores the original field value from the source, if needed for reference.
        /// </summary>
        [JsonIgnore]
        public string OriginalField { get; set; }

        /// <summary>
        /// Indicates whether this value has a coded type.
        /// </summary>
        public bool HasCodedType { get { return (TypeCC != null); } }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Value"/> class from a <see cref="JToken"/> and a list of <see cref="DataType"/>.
        /// </summary>
        /// <param name="jToken">The JSON token containing the value information.</param>
        /// <param name="dataTypeList">The list of <see cref="DataType"/> objects used to resolve the value's type.</param>
        /// <param name="referenceData">The optional <see cref="PIQIReferenceData"/> object used to resolve the codeable concept recognized systems.</param>
        public Value(JToken jToken, List<DataType> dataTypeList, PIQIReferenceData? referenceData)
        {
            // Initialize coding list
            CodingList = new List<Coding>();

            // Process text node
            if (jToken.SelectToken("text") == null && jToken.SelectToken("codings") == null)
            {
                // If no structured object, use the node value as text
                Text = jToken.Value<string>();
            }
            else
            {
                // Parse structured node
                Text = Utility.GetJSONString(jToken, "text");

                JToken codeItemTokens = jToken.SelectToken("codings");
                if (codeItemTokens != null)
                {
                    foreach (JToken codeItemToken in codeItemTokens.Children())
                    {
                        Coding item = new Coding(codeItemToken);
                        CodingList.Add(item);
                    }
                }

                // If there is display but not text, use first display for text
                if (string.IsNullOrEmpty(Text) && CodingList.Any())
                {
                    string text = CodingList.Where(t => !string.IsNullOrEmpty(t.CodeText)).FirstOrDefault()?.CodeText;
                    if (!string.IsNullOrEmpty(text)) Text = text;
                }

                // If there is text but not display, use text for display
                if (!string.IsNullOrEmpty(Text) && CodingList.Any(t => string.IsNullOrEmpty(t.CodeText)))
                {
                    List<Coding> noDisplayList = CodingList.Where(t => string.IsNullOrEmpty(t.CodeText)).ToList();
                    foreach (Coding coding in noDisplayList)
                        coding.CodeText = Text;
                }
            }
            if (referenceData != null) SetRecognizedCodeSystems(referenceData);

            // Process type node
            JToken typeToken = jToken.SelectToken("type");
            if (typeToken != null) TypeCC = new CodeableConcept(typeToken, referenceData);

            // Resolve DataType from TypeCC
            if (TypeCC != null) Type = dataTypeList.FirstOrDefault(dt => dt.Code == TypeCC.Text);
            if (Type == null) Type = dataTypeList.FirstOrDefault(dt => dt.Code == "ST");

            // Calculate numeric values based on type
            if (Type != null)
            {
                if (this.Type.IsNumeric && !this.Type.IsRange)
                {
                    double val;
                    if (double.TryParse(Text, out val)) ValueNumber = val;
                }
                if (this.Type.IsNumeric && this.Type.IsRange)
                {
                    double val;
                    List<string> bitList = new List<string>(Text.Split(new char[] { '^' }));
                    if (bitList.Count > 0) if (double.TryParse(bitList[0], out val)) ValueNumber = val;
                    if (bitList.Count > 1) if (double.TryParse(bitList[1], out val)) ValueNumber2 = val;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates each <see cref="Coding"/> in <see cref="CodingList"/> with recognized code system information 
        /// based on the provided <see cref="PIQIReferenceData"/>.
        /// </summary>
        /// <param name="referenceData">
        /// The <see cref="PIQIReferenceData"/> used to determine which code systems are recognized.
        /// </param>
        /// <remarks>
        /// For each <see cref="Coding"/> in <see cref="CodingList"/>, the method:
        /// <list type="bullet">
        ///   <item>Finds the first code system in <see cref="Coding.CodeSystemList"/> that is recognized by <paramref name="referenceData"/>.</item>
        ///   <item>Sets <see cref="Coding.HasRecognizedCodeSystem"/> to <c>true</c> if a recognized code system is found, otherwise <c>false</c>.</item>
        ///   <item>Assigns <see cref="Coding.RecognizedCodeSystem"/> to the recognized code system, or <c>null</c> if none are recognized.</item>
        /// </list>
        /// </remarks>
        public void SetRecognizedCodeSystems(PIQIReferenceData referenceData)
        {
            // Update each coding with recognized code system information
            foreach (Coding coding in CodingList)
            {
                var recognizedCodeSystem = coding.CodeSystemList?.FirstOrDefault(cs => referenceData.GetCodeSystem(cs) != null);
                coding.HasRecognizedCodeSystem = recognizedCodeSystem != null;
                coding.RecognizedCodeSystem = recognizedCodeSystem;
                if (recognizedCodeSystem != null) coding.CodeSystem = recognizedCodeSystem;
            }
        }

        #endregion
    }
}
