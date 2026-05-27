using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{

    /// <summary>
    /// SAM engine that evaluates whether a given element is considered "clean".
    /// </summary>
    /// <remarks>
    /// An element is deemed <c>clean</c> if none of its child evaluations have failed.
    /// The evaluation iterates through the child items of the supplied <see cref="EvaluationItem"/>
    /// and counts the number of failed criteria across those children. If the failure count is zero,
    /// the evaluation passes.
    /// </remarks>
    public class SAM_ElementIsClean : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ElementIsClean"/> class.
        /// </summary>
        /// <param name="sam">The SAM configuration/context for this evaluation.</param>
        /// <param name="samService">The SAM service used by the evaluation engine.</param>
        public SAM_ElementIsClean(SAM sam, SAMService samService)
            : base(sam, samService) { }


        /// <summary>
        /// Evaluates whether the supplied element is "clean" (i.e., has no failed child evaluations).
        /// </summary>
        /// <param name="request">
        /// The evaluation request whose <see cref="PIQISAMRequest.EvaluationObject"/> must be an
        /// <see cref="EvaluationItem"/>. The method inspects its <see cref="EvaluationItem.ChildDict"/>
        /// and aggregates failures from each child's <see cref="EvaluationItem.CriteriaResultDict"/>.
        /// </param>
        /// <returns>
        /// A <see cref="PIQISAMResponse"/> indicating success when no child criteria have failed,
        /// or failure otherwise. If an exception occurs, the response is returned in an error state.
        /// </returns>
        /// <remarks>
        /// The first parameter is always expected to be an <see cref="EvaluationItem"/>.
        /// A child contributes failures only if it <see cref="EvaluationItem.HasResults"/> is <c>true</c>.
        /// The pass condition is: total failed criteria across all children &lt; 1.
        /// </remarks>
        /// <exception cref="InvalidCastException">
        /// Thrown if <see cref="PIQISAMRequest.EvaluationObject"/> cannot be cast to <see cref="EvaluationItem"/>.
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

                // Clean is true if there are no child evaluations that failed
                int failCount = 0;
                foreach (EvaluationItem attrItem in evaluationItem.ChildDict.Values)
                {
                    if (!attrItem.HasResults) continue;
                    failCount += (attrItem.CriteriaResultDict.Values.Where(t => t.EvalFailed).Count());
                }

                // Evaluate
                passed = (failCount < 1);

                // Done
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
        public static string StaticMnemonic => "ELEMENT_ISCLEAN";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
