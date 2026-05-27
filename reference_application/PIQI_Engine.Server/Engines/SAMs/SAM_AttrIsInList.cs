using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if an attribute's value exists in a provided list.
    /// </summary>
    public class SAM_AttrIsInList : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_AttrIsInList"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_AttrIsInList(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the attribute of a message model item matches a list of valid values.
        /// </summary>
        /// <param name="request">
        /// A <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        /// <item><description>The <see cref="PIQISAMRequest.MessageObject"/> which should be a <see cref="MessageModelItem"/>.</description></item>
        /// <item><description>The <see cref="PIQISAMRequest.ParmList"/> which must include a parameter with the name "Valid Attribute List".</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation.
        /// The <see cref="PIQISAMResponse"/> contains:
        /// <list type="bullet">
        /// <item><description><c>True</c> in <see cref="PIQISAMResponse.Passed"/> if the attribute value matches one of the valid values.</description></item>
        /// <item><description><c>False</c> if it does not match or the data is empty.</description></item>
        /// <item><description>An error message in <see cref="PIQISAMResponse.ErrorMessage"/> if an exception occurred.</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if <see cref="PIQISAMRequest.ParmList"/> is <c>null</c> or does not contain the required "Valid Attribute List" parameter.
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

                // Only check if there is data
                if (!string.IsNullOrEmpty(data?.Text))
                {
                    // Process required parms
                    if (request.ParmList == null) throw new Exception("Parameter list was not supplied");
                    Tuple<string, string> arg1 = request.ParmList.Where(t => t.Item1 == "LIST_CSV").FirstOrDefault();
                    if (arg1 == null) throw new Exception("[List CSV] parameter not found");
                    string arg1Value = arg1.Item2;

                    // Split the list and evaluate if the data exists in it (case-insensitive)
                    arg1Value = arg1Value.ToUpper();
                    List<string> valuesList = Utility.Split(arg1Value);
                    passed = valuesList.Any(t => t.Equals(data.Text, StringComparison.CurrentCultureIgnoreCase));
                }

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
        public static string StaticMnemonic => "ATTR_INLIST";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
