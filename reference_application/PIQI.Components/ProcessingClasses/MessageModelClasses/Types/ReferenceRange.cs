using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PIQI.Components.Services;
using static System.Net.Mime.MediaTypeNames;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a reference range for a value, including optional textual representation, low and high values.
    /// Inherits from <see cref="BaseText"/>.
    /// </summary>
    public class ReferenceRange : BaseText
    {
        #region Properties

        /// <summary>
        /// The lower bound of the reference range.
        /// </summary>
        [JsonProperty(PropertyName = "lowValue")]
        public string LowValue { get; set; }

        /// <summary>
        /// The upper bound of the reference range.
        /// </summary>
        [JsonProperty(PropertyName = "highValue")]
        public string HighValue { get; set; }

        /// <summary>
        /// Indicates whether the textual representation (<see cref="Text"/>) is present.
        /// </summary>
        [JsonIgnore]
        public bool HasText { get { return !string.IsNullOrWhiteSpace(Text); } }

        /// <summary>
        /// Indicates whether the low value is present.
        /// </summary>
        [JsonIgnore]
        public bool HasLow { get { return !string.IsNullOrWhiteSpace(LowValue); } }

        /// <summary>
        /// Indicates whether the high value is present.
        /// </summary>
        [JsonIgnore]
        public bool HasHigh { get { return !string.IsNullOrWhiteSpace(HighValue); } }

        /// <summary>
        /// Indicates whether both low and high values are present, marking the reference range as complete.
        /// </summary>
        [JsonIgnore]
        public bool IsComplete { get { return (HasLow && HasHigh); } }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor. Initializes an empty reference range.
        /// </summary>
        public ReferenceRange() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceRange"/> class from a <see cref="JToken"/>.
        /// </summary>
        /// <param name="pToken">The JSON token containing reference range information.</param>
        public ReferenceRange(JToken pToken)
        {
            Text = Utility.GetJSONString(pToken, "text", "Text");
            LowValue = Utility.GetJSONString(pToken, "lowValue", "LowValue");
            HighValue = Utility.GetJSONString(pToken, "highValue", "HighValue");
        }

        #endregion
    }
}
