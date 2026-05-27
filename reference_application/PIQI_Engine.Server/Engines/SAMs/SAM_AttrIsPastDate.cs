using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if an attribute's value is a date in the past.
    /// </summary>
    public class SAM_AttrIsPastDate : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_AttrIsPastDate"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_AttrIsPastDate(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the text value of a message attribute represents a valid <see cref="DateTime"/>
        /// that occurs in the past relative to the current system time.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing the message object to evaluate. 
        /// The <c>MessageObject</c> property must be a <see cref="MessageModelItem"/> whose 
        /// <c>MessageData</c> is of type <see cref="BaseText"/>. 
        /// The <see cref="BaseText.DateTimeValue"/> method is used to parse the datetime value.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result. 
        /// The <see cref="PIQISAMResponse"/> indicates whether the datetime value is in the past,
        /// or contains an error message if evaluation fails.
        /// </returns>
        /// <remarks>
        /// The value is considered valid if:
        /// <list type="bullet">
        /// <item><description>It can be successfully cast to <see cref="DateTime"/>.</description></item>
        /// <item><description>The datetime occurs before the current system time (<see cref="DateTime.Now"/>).</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown if the <see cref="PIQISAMRequest.MessageObject"/> cannot be cast to <see cref="MessageModelItem"/>,
        /// if <see cref="MessageModelItem.MessageData"/> is not a <see cref="BaseText"/>, 
        /// or if the value cannot be parsed as a <see cref="DateTime"/>.
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

                // Cast to DateTime
                DateTime? dateTime = data.DateTimeValue();
                if (dateTime == null)
                    throw new Exception("Data parm was not a datetime. Check the SAM dependencies");

                // Check if the date is in the past
                DateTime now = DateTime.Now;
                passed = (now > dateTime.Value);

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
        public static string StaticMnemonic => "ATTR_ISPASTDATE";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
