using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM that evaluates whether a given <see cref="Value"/> is of a qualitative type.
    /// By default, this checks for value types "CE", "CWE", "CD", or "ST".
    /// </summary>
    public class SAM_ValueIsQualitative : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ValueIsQualitative"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ValueIsQualitative(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the <see cref="Value"/> contained in the request is of a qualitative type.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> 
        ///     whose <see cref="MessageModelItem.MessageData"/> is a <see cref="Value"/>.
        ///   </item>
        ///   <item>
        ///     Optional entries in <see cref="PIQISAMRequest.ParmList"/> to override the default allowed type codes.
        ///     If not provided, the default list "CE|CWE|CD|ST" is used.
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation.  
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if the <see cref="Value"/> type is in the allowed qualitative list.</item>
        ///   <item><c>Failed</c> if the type is not in the list.</item>
        ///   <item><c>Errored</c> if the <see cref="MessageModelItem.MessageData"/> is not a <see cref="Value"/> or another exception occurs.</item>
        /// </list>
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

                // Validate the data format
                if (data is not Value)
                    throw new Exception("ValueTypeIsQualitative expects an observation value.");

                // Cast data as observation value
                Value val = (Value)data;

                // Process required parms
                // Note: for now if we don't get the required parms we use a hard-coded list. this is a stopgap
                string valueText = "CE|CWE|CD|ST|FT|TX";

                if (request.ParmList != null)
                {
                    Tuple<string, string> arg1 = request.ParmList.Where(t => t.Item1 == "Valid Attribute List").FirstOrDefault();
                    if (arg1 != null)
                    {
                        valueText = arg1.Item2;
                    }
                }

                // Split param into list
                List<string> valuesList = Utility.Split(valueText);

                // Evaluate
                passed = valuesList != null && val.Type?.Code != null
                    && valuesList.Any(t => t.Equals(val.Type.Code, StringComparison.CurrentCultureIgnoreCase));

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
        public static string StaticMnemonic => "OBSERVATIONVALUE_ISQUALITATIVE";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
