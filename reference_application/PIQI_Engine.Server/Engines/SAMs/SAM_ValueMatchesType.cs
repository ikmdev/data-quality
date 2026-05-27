using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM that evaluates whether a given <see cref="Value"/> instance
    /// has a type and content consistent with its expected type definition.
    /// </summary>
    public class SAM_ValueMatchesType : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ValueMatchesType"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object that defines configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ValueMatchesType(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the <see cref="Value"/> contained in the request
        /// is valid for its declared type.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> that provides:
        /// <list type="bullet">
        ///   <item>
        ///     The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/>
        ///     whose <see cref="MessageModelItem.MessageData"/> is a <see cref="Value"/>.
        ///   </item>
        ///   <item>
        ///     Optional <see cref="PIQISAMRequest.ParmList"/> entries, which may include a
        ///     delimited string of allowed type codes. (Currently not used in evaluation.)
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation.  
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if the <see cref="Value"/> type is valid and its required fields are present.</item>
        ///   <item><c>Failed</c> if the type is missing, unknown, or inconsistent with its content (e.g., numeric without a number, coded without codings).</item>
        ///   <item><c>Errored</c> if the evaluation encounters an exception (e.g., unexpected input type).</item>
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

                // Validate the data type
                if (data is not Value val)
                    throw new Exception("ValueMatchesType expects an observation value.");

                // Evaluate type rules
                if (val.Type == null || val.Type.Code == "UNK") // Fail if type is missing or unknown
                    passed = false;
                else if (val.Type.IsCoded && !val.HasCodedItems) // Fail for coded types without codings
                    passed = false;
                else if (val.Type.IsNumeric && val.ValueNumber == null) // Fail for numeric types without ValueNumber
                    passed = false;
                else if (val.Type.IsNumeric && val.Type.IsRange && (val.ValueNumber == null || val.ValueNumber2 == null)) // Fail for numeric range with missing numbers
                    passed = false;
                else if (string.IsNullOrWhiteSpace(val.Text)) // Fail if text is missing for other types
                    passed = false;
                else
                    passed = true;

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
        public static string StaticMnemonic => "OBSERVATIONVALUE_MATCHESTYPE";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
