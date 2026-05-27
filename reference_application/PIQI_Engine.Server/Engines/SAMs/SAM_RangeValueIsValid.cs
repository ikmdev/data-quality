using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM that checks whether a <see cref="ReferenceRange"/> value is valid.
    /// This SAM is currently not implemented and will return an error if evaluated.
    /// </summary>
    public class SAM_RangeValueIsValid : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_RangeValueIsValid"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_RangeValueIsValid(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the <see cref="Value"/> contained in the request is a valid <see cref="ReferenceRange"/>.
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
        ///   <item><c>Errored</c> because this SAM is not yet implemented.</item>
        /// </list>
        /// </returns>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // *** This implementation is incomplete
                throw new Exception("SAM not implemented.");

                // The following code is placeholder and not currently reachable
                MessageModelItem item = (MessageModelItem)request.EvaluationObject;
                BaseText data = (BaseText)item.MessageData;

                if (data is not ReferenceRange)
                    throw new Exception("RangeValueIsValid expects a reference range value.");

                ReferenceRange rr = (ReferenceRange)data;
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
        public static string StaticMnemonic => "RANGEVALUE_ISVALID";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
