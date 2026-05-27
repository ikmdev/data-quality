using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIQI.Components.Services;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a codeable concept, which may contain free text and one or more codings.
    /// </summary>
    public class CodeableConcept : BaseText
    {
        #region Properties

        /// <summary>
        /// List of codings associated with this concept.
        /// </summary>
        [JsonProperty(PropertyName = "codings")]
        public List<Coding> CodingList { get; set; }

        /// <summary>
        /// Determines the state of the codeable concept.
        /// </summary>
        [JsonIgnore]
        public CodeableConceptStateEnum ConceptState
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Text) && CodingList.Count > 0) return CodeableConceptStateEnum.Both;
                else if (!string.IsNullOrWhiteSpace(Text) && CodingList.Count < 1) return CodeableConceptStateEnum.TextOnly;
                else if (string.IsNullOrWhiteSpace(Text) && CodingList.Count > 0) return CodeableConceptStateEnum.ConceptsOnly;
                else return CodeableConceptStateEnum.None;
            }
        }

        /// <summary>
        /// Indicates whether the concept has text.
        /// </summary>
        [JsonIgnore]
        public bool HasText { get { return !string.IsNullOrWhiteSpace(this.Text); } }

        /// <summary>
        /// Indicates whether the concept has codings.
        /// </summary>
        [JsonIgnore]
        public bool HasCodedItems { get { return CodingList.Count > 0; } }

        /// <summary>
        /// Indicates whether the concept has all required items.
        /// </summary>
        [JsonIgnore]
        public bool HasCompleteItems { get; set; }

        /// <summary>
        /// Indicates whether the concept contains recognized items.
        /// </summary>
        [JsonIgnore]
        public bool HasRecognizedItems { get; set; }

        /// <summary>
        /// Indicates whether the concept contains valid items.
        /// </summary>
        [JsonIgnore]
        public bool HasValidItems { get; set; }

        /// <summary>
        /// Indicates whether the concept contains active items.
        /// </summary>
        [JsonIgnore]
        public bool HasActiveItems { get; set; }

        /// <summary>
        /// Indicates whether the concept contains interoperable items.
        /// </summary>
        [JsonIgnore]
        public bool HasInteroperableItems { get; set; }

        /// <summary>
        /// Indicates whether the concept contains semantically correct items.
        /// </summary>
        [JsonIgnore]
        public bool HasSemanticItems { get; set; }

        /// <summary>
        /// Marks if a FHIR server lookup has been called for this codeable concept.
        /// </summary>
        [JsonIgnore]
        public bool FHIRServerCalled { get; set; } = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CodeableConcept() { }

        /// <summary>
        /// Initializes a new instance from a <see cref="JToken"/> representing a codeable concept.
        /// </summary>
        /// <param name="jToken">The JSON token to parse.</param>
        /// <param name="referenceData">The reference data with recognized code systems.</param>
        public CodeableConcept(JToken jToken, PIQIReferenceData? referenceData)
        {
            // Initialize lists
            CodingList = new List<Coding>();

            // Handle case where JSON has no text or codings
            if (jToken.SelectToken("text") == null && jToken.SelectToken("codings") == null)
            {
                Text = (string)((JProperty)jToken.Children().First()).Value;
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
                        if (item.CodeText == null) item.CodeText = Text;
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

        }

        /// <summary>
        /// Initializes a new instance from a <see cref="JProperty"/> representing a codeable concept.
        /// </summary>
        /// <param name="jProperty">The JSON property to parse.</param>
        /// /// <param name="referenceData">The reference data with recognized code systems.</param>
        public CodeableConcept(JProperty jProperty, PIQIReferenceData? referenceData)
        {
            // Initialize lists
            CodingList = new List<Coding>();

            if (jProperty.SelectToken("text") == null && jProperty.SelectToken("codings") == null)
            {
                Text = jProperty.Value.ToString();
            }
            else
            {
                Text = Utility.GetJSONString(jProperty, "text");

                JToken codeItemTokens = jProperty.SelectToken("codings");
                if (codeItemTokens != null)
                {
                    foreach (JToken codeItemToken in codeItemTokens.Children())
                    {
                        Coding item = new Coding(codeItemToken);
                        CodingList.Add(item);
                    }
                }

                if (string.IsNullOrEmpty(Text) && CodingList.Any())
                {
                    string text = CodingList.Where(t => !string.IsNullOrEmpty(t.CodeText)).FirstOrDefault()?.CodeText;
                    if (!string.IsNullOrEmpty(text)) Text = text;
                }

                if (!string.IsNullOrEmpty(Text) && CodingList.Any(t => string.IsNullOrEmpty(t.CodeText)))
                {
                    List<Coding> noDisplayList = CodingList.Where(t => string.IsNullOrEmpty(t.CodeText)).ToList();
                    foreach (Coding coding in noDisplayList)
                        coding.CodeText = Text;
                }
            }
            if (referenceData != null) SetRecognizedCodeSystems(referenceData);
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
