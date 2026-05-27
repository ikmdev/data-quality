using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;
using System.Text.RegularExpressions;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if an attribute's value matches a specified regular expression pattern.
    /// </summary>
    public class SAM_AttrMatchesRegex : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_AttrMatchesRegex"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_AttrMatchesRegex(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the provided <see cref="MessageModelItem"/>'s attribute value
        /// matches the specified regular expression pattern.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     <see cref="PIQISAMRequest.MessageObject"/> – expected to be a <see cref="MessageModelItem"/>
        ///     whose <see cref="MessageModelItem.MessageData"/> is a <see cref="BaseText"/> containing the value to check.
        ///   </item>
        ///   <item>
        ///     Optional entries in <see cref="PIQISAMRequest.ParmList"/>, 
        ///     where the second parameter is the regex pattern string to evaluate against.
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation.
        /// The response will indicate:
        /// <list type="bullet">
        ///   <item><c>Done(true)</c> if the attribute value matches the regex pattern.</item>
        ///   <item><c>Done(false)</c> if the attribute value does not match the regex pattern.</item>
        ///   <item><c>Error</c> if the input is invalid or an exception occurs.</item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the <see cref="MessageModelItem"/> does not contain a <see cref="BaseText"/> value
        /// or if the regex pattern parameter is invalid.
        /// </exception>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // Set the message model item
                EvaluationItem evaluationItem = (EvaluationItem)request.EvaluationObject;
                MessageModelItem item = evaluationItem?.MessageItem;

                // Access the attribute's message data
                BaseText data = (BaseText)item.MessageData;

                // Get the regex pattern parameter
                if (request.ParmList == null) throw new Exception("Parameter list was not supplied");
                Tuple<string, string>? arg1 = request.ParmList.Where(t => t.Item1 == "CUSTOM_REGULAR_EXPRESSION").FirstOrDefault();
                if (arg1 == null) throw new Exception("[Custom Regular Expression] parameter not found");
                string pattern = arg1.Item2;

                // Evaluate if the data matches the regex
                Regex regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
                Match match = regex.Match(data.Text);
                passed = match.Success;

                // Update result
                result.Done(passed);
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Gets the mnemonic code for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "ATTR_MATCHESREGEX";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
