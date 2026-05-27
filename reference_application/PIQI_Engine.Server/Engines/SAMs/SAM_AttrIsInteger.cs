using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if an attribute's value is a valid integer.
    /// </summary>
    public class SAM_AttrIsInteger : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_AttrIsInteger"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_AttrIsInteger(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the text value of a message attribute represents a valid integer.(MessageModelItem)request.EvaluationObject;
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing the message object to evaluate. 
        /// The <c>MessageObject</c> property must be a <see cref="MessageModelItem"/> whose 
        /// <c>MessageData</c> is of type <see cref="BaseText"/>. 
        /// The <see cref="BaseText.IsInt"/> method is used to validate the integer value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result. 
        /// The <see cref="PIQISAMResponse"/> indicates whether the text value is a valid integer,
        /// or contains an error message if evaluation fails.
        /// </returns>
        /// <remarks>
        /// The value is considered valid if it can be successfully parsed as an integer
        /// using <see cref="BaseText.IsInt"/>.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if the <see cref="PIQISAMRequest.MessageObject"/> cannot be cast to <see cref="MessageModelItem"/>
        /// or if <see cref="MessageModelItem.MessageData"/> is not a <see cref="BaseText"/>.
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

                // Check if the data is a valid integer
                passed = data.IsInt();

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
        public static string StaticMnemonic => "ATTR_ISINTEGER";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
