using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM that checks whether all required parameters for the evaluation are valid (non-null/non-empty).
    /// Returns <c>Succeeded</c> if all required parameters are present; otherwise, <c>Failed</c>.
    /// </summary>
    public class SAM_EvalIsValid : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_EvalIsValid"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_EvalIsValid(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether all required SAM parameters in the <see cref="PIQISAMRequest"/> are valid (non-null and non-empty).
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> whose <see cref="MessageModelItem.MessageData"/> may be relevant for parameter checks.</item>
        ///   <item>Optional entries in <see cref="PIQISAMRequest.ParmList"/> representing the SAM parameters to validate.</item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if all required parameters are present and non-empty.</item>
        ///   <item><c>Failed</c> if any required parameter is missing or empty.</item>
        ///   <item><c>Errored</c> if an exception occurs during evaluation.</item>
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

                // Evaluate each required SAM parameter
                int parmIndex = 0;
                foreach (SAMParameter samParameter in SAMObject.Parameters)
                {
                    if (request.ParmList == null) passed = false;
                    Tuple<string, string> parameter = request.ParmList.Where(t => t.Item1 == samParameter.Name).FirstOrDefault();
                    if (parameter == null) passed = false;
                    if (string.IsNullOrEmpty(parameter?.Item2)) passed = false;
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
        public static string StaticMnemonic => "EVAL_ISVALID";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
