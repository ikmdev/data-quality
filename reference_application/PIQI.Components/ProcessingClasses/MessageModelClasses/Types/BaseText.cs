using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Base class for all attribute types in a message entity.
    /// </summary>
    public class BaseText
    {
        #region Properties

        /// <summary>
        /// The textual representation of the value.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseText() { }

        /// <summary>
        /// Initializes a new instance with the specified text.
        /// </summary>
        /// <param name="text">The text to store in this object.</param>
        public BaseText(string text)
        {
            Text = text;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the text representation of this object.
        /// </summary>
        public override string ToString()
        {
            return this.Text;
        }

        /// <summary>
        /// Attempts to parse the text as an integer.
        /// </summary>
        /// <returns>The integer value if successful; otherwise, null.</returns>
        public int? IntValue()
        {
            int x = 0;
            bool ret = int.TryParse(Text, out x);
            return (ret ? x : null);
        }

        /// <summary>
        /// Determines whether the text can be parsed as an integer.
        /// </summary>
        public bool IsInt()
        {
            return (IntValue() != null);
        }

        /// <summary>
        /// Attempts to parse the text as a floating-point number.
        /// </summary>
        /// <returns>The float value if successful; otherwise, null.</returns>
        public float? FloatValue()
        {
            float x = 0;
            bool ret = float.TryParse(Text, out x);
            return (ret ? x : null);
        }

        /// <summary>
        /// Determines whether the text can be parsed as a float.
        /// </summary>
        public bool IsFloat()
        {
            return (FloatValue() != null);
        }

        /// <summary>
        /// Attempts to parse the text as a DateTime.
        /// </summary>
        /// <returns>The DateTime value if successful; otherwise, null.</returns>
        public DateTime? DateTimeValue()
        {
            DateTime x = DateTime.MinValue;
            bool ret = DateTime.TryParse(Text, out x);
            return (ret ? x : null);
        }

        /// <summary>
        /// Determines whether the text represents a valid DateTime.
        /// </summary>
        public bool IsDateTime()
        {
            DateTime x = DateTime.MinValue;

            // Try the default parsing
            bool isDateTime = DateTime.TryParse(Text, out x);

            if (!isDateTime)
            {
                // Try a no-delimiters pattern
                string format = "yyyyMMddHHmmss";
                isDateTime = DateTime.TryParseExact(Text, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out x);
            }

            if (!isDateTime)
            {
                // Try regex patterns
                List<string> patternList = new List<string>
                {
                    @"^(0?[1-9]|1[0-2])[- /.](0?[1-9]|[12][0-9]|3[01])[- /.](\d{4})$",
                    @"^\d{8}(0[0-9]|1[0-9]|2[0-3])([0-5][0-9]){2}$",
                    @"^\d{8}(0[0-9]|1[0-9]|2[0-3])([0-5][0-9]){2}$",
                    @"^\d{8}(0[0-9]|1[0-9]|2[0-3])([0-5][0-9]){2}-((0[0-9]|1[0-9]|2[0-3])([0-5][0-9]){2})$",
                    @"^((19|20)\d{2})(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])([01]\d|2[0-3])([0-5]\d)$"
                };
                int index = 0;
                while (index < patternList.Count && !isDateTime)
                {
                    Match m = Regex.Match(Text, patternList[index++]);
                    if (m.Success) isDateTime = true;
                }
            }

            return isDateTime;
        }

        /// <summary>
        /// Attempts to parse the text as a boolean value.
        /// </summary>
        /// <returns>True or False if recognized; otherwise, null.</returns>
        public bool? BoolValue()
        {
            if (string.IsNullOrEmpty(Text)) return null;
            HashSet<string> trueSet = new HashSet<string>() { "1", "TRUE", "T", "YES", "Y" };
            HashSet<string> falseSet = new HashSet<string>() { "0", "FALSE", "F", "NO", "N" };
            if (trueSet.Contains(Text.ToUpper())) return true;
            else if (falseSet.Contains(Text.ToUpper())) return false;
            else return null;
        }

        /// <summary>
        /// Determines whether the text represents a boolean value.
        /// </summary>
        public bool IsBool()
        {
            return BoolValue() != null;
        }

        #endregion
    }
}
