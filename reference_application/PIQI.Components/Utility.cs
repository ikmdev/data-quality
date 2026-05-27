using Newtonsoft.Json.Linq;
using PIQI.Components.Models;

namespace PIQI.Components.Services
{
    /// <summary>
    /// Utility class providing helper methods for type conversion, string manipulation, Levenshtein distance calculation, and JSON operations.
    /// </summary>
    public class Utility
    {
        #region Conversion

        /// <summary>
        /// Converts an object to a nullable <see cref="DateTime"/>. Returns null if the object is null or cannot be converted.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>A nullable <see cref="DateTime"/> representing the object's value, or null if conversion fails.</returns>
        public static DateTime? ObjNullableDateTime(object value)
		{
			DateTime result = DateTime.MinValue;
			if (value != null)
			{
				if (value is DateTime)
					result = (DateTime)value;
				else
					DateTime.TryParse(value.ToString(), out result);
			}

			if (result == DateTime.MinValue)
				return null;
			else
				return result;
		}

        #endregion

        #region String

        /// <summary>
        /// Splits a string into a list of substrings using '|' as the primary delimiter and ',' as a fallback.
        /// </summary>
        /// <param name="input">The string to split.</param>
        /// <returns>A list of substrings, or null if the input is null or whitespace.</returns>
        public static List<string> Split(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            List<string> valuesList = Split_Internal(input, '|');
            if (valuesList.Count < 2)
                valuesList = Split_Internal(input, ',');
            return valuesList;
        }

        private static List<string> Split_Internal(string input, char delimiter)
        {
            List<string> valuesList = new List<string>();
            string current = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == delimiter)
                {
                    valuesList.Add(current);
                    current = "";
                }
                else
                    current += input[i];
            }
            if (!string.IsNullOrWhiteSpace(current)) valuesList.Add(current);
            return valuesList;
        }

        /// <summary>
        /// Computes the Levenshtein distance between two strings.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="target">The target string.</param>
        /// <returns>The number of single-character edits required to change the source into the target.</returns>
        public static int ComputeLevenshteinDistance(string source, string target)
        {
            if (source != null && target != null && source == target) return 0;

            int sourceWordCount = source?.Length ?? 0;
            int targetWordCount = target?.Length ?? 0;

            // Step 1 
            if (sourceWordCount == null || sourceWordCount == 0) return targetWordCount;
            if (targetWordCount == null || targetWordCount == 0) return sourceWordCount;
            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2 
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3 
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4 
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }
            return distance[sourceWordCount, targetWordCount];
        }
        #endregion

        #region JSON

        #region Get methods

        /// <summary>
        /// Retrieves a <see cref="JToken"/> by name or alternate names from a parent token.
        /// </summary>
        /// <param name="token">Parent <see cref="JToken"/> to search.</param>
        /// <param name="tokenName">Primary token name to retrieve.</param>
        /// <param name="altArray">Optional alternate token names to try.</param>
        /// <returns>The found <see cref="JToken"/>, or null if not found.</returns>
        public static JToken GetJSONToken(JToken token, string tokenName, params string[] altArray)
        {
            // Exit condition
            if (!token.HasValues) return null;

            // Get the specified token
            //JToken myToken = token.SelectToken(tokenName);
            JToken myToken = token
                .Children<JProperty>()
                .FirstOrDefault(p =>
                    string.Equals(p.Name, tokenName, StringComparison.OrdinalIgnoreCase))
                ?.Value;
            if (myToken != null) return myToken;

            // If we have alternate paths, explore them
            if (myToken == null && altArray != null && altArray.Length > 0)
            {
                for (int i = 0; i < altArray.Length; i++)
                {
                    myToken = token.SelectToken(altArray[i]);
                    if (myToken != null) return myToken;
                }
            }

            // Failure
            return null;
        }

        /// <summary>
        /// Retrieves the string value of a token by name or alternate names.
        /// </summary>
        /// <param name="token">Parent <see cref="JToken"/> to search.</param>
        /// <param name="attrName">Primary attribute name to retrieve.</param>
        /// <param name="altArray">Optional alternate attribute names to try.</param>
        /// <returns>The string value, or null if not found.</returns>
        public static string GetJSONString(JToken token, string attrName, params string[] altArray)
		{
			JToken myToken = token.SelectToken(attrName);
			if (myToken == null)
			{
				for (int i = 0; i < altArray.Length; i++)
				{
					myToken = token.SelectToken(altArray[i]);
					if (myToken != null) break;
				}
			}

			return (myToken == null ? null : myToken.Value<string>());
		}

        /// <summary>
        /// Retrieves a list of string values from a token by name and optional alternate names.
        /// </summary>
        /// <param name="token">Parent <see cref="JToken"/> to search.</param>
        /// <param name="attrName">Primary attribute name to retrieve.</param>
        /// <param name="altArray">Optional alternate attribute names to try.</param>
        /// <returns>A list of string values.</returns>
        public static List<string> GetJSONStringList(JToken token, string attrName, params string[] altArray)
        {
            List<string> list = new List<string>();
            JToken myToken = token.SelectToken(attrName);
            if (myToken != null) list.Add(myToken.Value<string>());

            for (int i = 0; i < altArray.Length; i++)
            {
                myToken = token.SelectToken(altArray[i]);
                if (myToken != null) list.Add(myToken.Value<string>());
            }
            return list;
        }
        #endregion

        #region Add methods
        /// <summary>
        /// Adds a new <see cref="JObject"/> to a <see cref="JArray"/>.
        /// </summary>
        /// <param name="parent">Parent <see cref="JArray"/> to add the object to.</param>
        /// <returns>The newly created <see cref="JObject"/>.</returns>
        public static JObject JSON_AddObject(JArray parent)
        {
            JObject obj = new JObject();
            parent.Add(obj);
            return obj;
        }

        /// <summary>
        /// Adds a new <see cref="JObject"/> to a parent <see cref="JObject"/> with the specified name.
        /// </summary>
        /// <param name="parent">Parent <see cref="JObject"/>.</param>
        /// <param name="name">Name of the new object.</param>
        /// <returns>The newly created <see cref="JObject"/>.</returns>
        public static JObject JSON_AddObject(JObject parent, string name)
        {
            JObject obj = new JObject();
            parent.Add(name, obj);
            return obj;
        }

        /// <summary>
        /// Adds a new <see cref="JObject"/> to a parent <see cref="JObject"/> with the specified name.
        /// </summary>
        /// <param name="parent">Parent <see cref="JObject"/>.</param>
        /// <param name="name">Name of the new object.</param>
        /// <returns>The newly created <see cref="JObject"/>.</returns>
        public static JObject JSON_AddObject(JArray parent, string name)
        {
            JObject obj = new JObject();
            parent.Add(obj);
            return obj;
        }

        /// <summary>
        /// Adds a new <see cref="JArray"/> to a parent <see cref="JObject"/> with the specified name.
        /// </summary>
        /// <param name="parent">Parent <see cref="JObject"/>.</param>
        /// <param name="name">Name of the new array.</param>
        /// <returns>The newly created <see cref="JArray"/>.</returns>
        public static JArray JSON_AddArray(JObject parent, string name)
        {
            JArray array = new JArray();
            parent.Add(name, array);
            return array;
        }

        /// <summary>
        /// Adds a CodeableConcept object to a parent <see cref="JObject"/>.
        /// </summary>
        /// <param name="concept">The <see cref="CodeableConcept"/> to add.</param>
        /// <returns>The newly created <see cref="JObject"/> representing the CodeableConcept.</returns>
        public static JObject JSON_AddCodeableConceptObject(CodeableConcept concept)
        {
            JObject cc = new JObject();
            if (concept.CodingList != null && concept.CodingList.Count > 0)
            {
                JArray codings = new JArray();
                foreach (Coding coding in concept.CodingList)
                {
                    JObject codingToken = new JObject();
                    codingToken.Add("system", coding.CodeSystem);
                    codingToken.Add("code", coding.CodeValue);
                    codingToken.Add("display", coding.CodeText);
                    codings.Add(codingToken);
                }
                cc.Add("codings", codings);
            }
            cc.Add("text", concept.Text);

            return cc;
        }

        /// <summary>
        /// Adds a <see cref="Value"/> object to a parent <see cref="JObject"/>.
        /// </summary>
        /// <param name="value">The <see cref="Value"/> object to add.</param>
        /// <returns>The newly created <see cref="JObject"/> representing the value.</returns>

        public static JObject JSON_AddValueObject(Value value)
        {
            JObject v = new JObject();
            v.Add("text", value.Text ?? "");

            if (value.TypeCC != null)
            {
                var cc = JSON_AddCodeableConceptObject(value.TypeCC);
                if (cc != null)
                {
                    v.Add("type", cc);
                }
            }

            return v;
        }

        /// <summary>
        /// Adds a <see cref="ReferenceRange"/> object to a parent <see cref="JObject"/>.
        /// </summary>
        /// <param name="range">The <see cref="ReferenceRange"/> object to add.</param>
        /// <returns>The newly created <see cref="JObject"/> representing the reference range.</returns>

        public static JObject JSON_AddRefRangeObject(ReferenceRange range)
        {
            JObject rr = new JObject();
            rr.Add("text", range.Text ?? "");
            rr.Add("lowValue", range.LowValue);
            rr.Add("highValue", range.HighValue);

            return rr;
        }

        #endregion

        #endregion
    }
}
