using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// Default SAM used as a placeholder when a requested SAM has not been implemented.
    /// Always returns <c>Failed</c> and can be used for logging or debugging purposes.
    /// </summary>
    public class SAM_Default : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_Default"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_Default(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates the provided <see cref="PIQISAMRequest"/> and always returns <c>Failed</c>.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>The <see cref="PIQISAMRequest.MessageObject"/> (expected to be a <see cref="MessageModelItem"/> whose <see cref="MessageModelItem.MessageData"/> may be relevant).</item>
        ///   <item>Optional <see cref="PIQISAMRequest.ParmList"/> entries (currently unused).</item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// The response always indicates:
        /// <list type="bullet">
        ///   <item><c>Failed</c> to signal that this SAM is not implemented.</item>
        /// </list>
        /// </returns>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
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
        public static string StaticMnemonic => "default";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
