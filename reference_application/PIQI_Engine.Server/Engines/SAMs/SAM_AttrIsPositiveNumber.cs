using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if an attribute's value is a positive number.
    /// </summary>
    public class SAM_AttrIsPositiveNumber : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_AttrIsPositiveNumber"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_AttrIsPositiveNumber(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the text value of a message attribute is a numeric value and positive.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing the message object to evaluate. 
        /// The <c>MessageObject</c> property must be a <see cref="MessageModelItem"/> whose 
        /// <c>MessageData</c> is of type <see cref="BaseText"/>. 
        /// The <see cref="BaseText.IsFloat"/> and <see cref="BaseText.FloatValue"/> methods are used
        /// to validate and retrieve the numeric value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result. 
        /// The <see cref="PIQISAMResponse"/> indicates whether the value is numeric and positive,
        /// or contains an error message if evaluation fails.
        /// </returns>
        /// <remarks>
        /// The value is considered valid if:
        /// <list type="bullet">
        /// <item><description>It can be successfully parsed as a float.</description></item>
        /// <item><description>The numeric value is greater than zero.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if the <see cref="PIQISAMRequest.MessageObject"/> cannot be cast to <see cref="MessageModelItem"/>,
        /// if <see cref="MessageModelItem.MessageData"/> is not a <see cref="BaseText"/>, 
        /// or if the value is not numeric.
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

                // Validate that the data is numeric
                if (!data.IsFloat())
                    throw new Exception("AttrIsPositiveNumber expects numeric data. Check the dependency tree.");

                // Check if the numeric value is positive
                passed = (data.FloatValue() > 0);

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
        public static string StaticMnemonic => "ATTR_ISPOSITIVENUMBER";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
