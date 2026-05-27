using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM that checks whether a given <see cref="Value"/>'s type is included in a provided list of allowed types.
    /// </summary>
    public class SAM_ValueTypeInList : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ValueTypeInList"/> class.
        /// </summary>
        /// <param name="sam">The parent SAM object.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ValueTypeInList(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the <see cref="Value"/> contained in the provided request 
        /// matches one of the allowed type codes.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> whose <see cref="MessageModelItem.MessageData"/> is a <see cref="Value"/>.</item>
        ///   <item>Optional entries in <see cref="PIQISAMRequest.ParmList"/>, where one parameter contains the delimited string of allowed type codes.</item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation. 
        /// The response indicates success if the <see cref="Value"/> type is included in the allowed list; otherwise, it indicates failure.
        /// </returns>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // Set the message model item
                EvaluationItem evaluationItem = (EvaluationItem)request.EvaluationObject;
                MessageModelItem item = evaluationItem?.MessageItem;

                // Access the item's message data
                BaseText data = (BaseText)item.MessageData;

                // Validate the data type
                if (data is not Value val)
                    throw new Exception("ValueTypeInList expects an observation value.");

                // Get allowed types parameter
                if (request.ParmList == null) throw new Exception("Parameter list was not supplied");
                Tuple<string, string> arg1 = request.ParmList.Where(t => t.Item1 == "VALUE_TYPE_CSV").FirstOrDefault();
                if (arg1 == null) throw new Exception("[Value Type CSV] parameter not found");
                string setMnemonic = arg1.Item2;
                string valueText = data.Text;

                // Split parameter into a list
                List<string> valuesList = Utility.Split(setMnemonic);

                // Evaluate
                passed = (val.Type != null
                          && valuesList.Any(t => t.Equals(val.Type.Code, StringComparison.CurrentCultureIgnoreCase)));

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
        public static string StaticMnemonic => "OBSERVATIONVALUETYPE_INLIST";

        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
