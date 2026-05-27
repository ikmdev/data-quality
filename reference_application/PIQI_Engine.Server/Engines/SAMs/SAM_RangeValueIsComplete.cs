using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM that checks if a <see cref="ReferenceRange"/> value is complete.
    /// Returns <c>Succeeded</c> if the range is complete, <c>Failed</c> otherwise.
    /// </summary>
    public class SAM_RangeValueIsComplete : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_RangeValueIsComplete"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_RangeValueIsComplete(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the <see cref="Value"/> contained in the request is a complete <see cref="ReferenceRange"/>.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> 
        ///     whose <see cref="MessageModelItem.MessageData"/> is a <see cref="ReferenceRange"/>.
        ///   </item>
        ///   <item>
        ///     Optional entries in <see cref="PIQISAMRequest.ParmList"/> (currently unused).
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation.
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if the <see cref="ReferenceRange"/> is complete.</item>
        ///   <item><c>Failed</c> if the <see cref="ReferenceRange"/> is incomplete.</item>
        ///   <item><c>Errored</c> if the message data is not a <see cref="ReferenceRange"/> or another exception occurs.</item>
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

                // Get the item's message data
                BaseText data = (BaseText)item.MessageData;

                // Validate the data format
                if (data is not ReferenceRange)
                    throw new Exception("RangeValueIsComplete expects a reference range value.");

                // Cast data as reference range
                ReferenceRange rr = (ReferenceRange)data;

                // Evaluate
                passed = rr.IsComplete;

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
        public static string StaticMnemonic => "RANGEVALUE_ISCOMPLETE";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
