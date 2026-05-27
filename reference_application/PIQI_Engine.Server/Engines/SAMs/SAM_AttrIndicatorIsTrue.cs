
using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM evaluation engine that determines whether a message's attribute indicator
    /// represents a <c>true</c> value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This evaluator inspects the <see cref="MessageModelItem"/> contained in the request's
    /// <see cref="PIQISAMRequest.EvaluationObject"/>. It expects the message data to be a
    /// <see cref="BaseText"/> and considers the indicator as <c>true</c> if:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///   The data is a <see cref="CodeableConcept"/> (sets an initial <c>true</c> state).
    ///   </description></item>
    ///   <item><description>
    ///   OR the textual value matches any of: <c>"T"</c>, <c>"True"</c>, <c>"Y"</c>, <c>"Yes"</c>, <c>"1"</c>
    ///   (case-insensitive).
    ///   </description></item>
    /// </list>
    /// <para>
    /// If the message data is null or has an empty <see cref="BaseText.Text"/>, the evaluation fails
    /// and an error is returned.
    /// </para>
    /// </remarks>
    public class SAM_AttrIndicatorIsTrue : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SAM_AttrIndicatorIsTrue"/>.
        /// </summary>
        /// <param name="sam">The SAM definition metadata.</param>
        /// <param name="samService">The SAM service providing dependencies/utilities.</param>
        public SAM_AttrIndicatorIsTrue(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Evaluates the provided request to determine if the attribute indicator resolves to a <c>true</c> value.
        /// </summary>
        /// <param name="request">
        /// The SAM request containing the <see cref="PIQISAMRequest.EvaluationObject"/> which must be a
        /// <see cref="MessageModelItem"/> with <see cref="MessageModelItem.MessageData"/> of type <see cref="BaseText"/>.
        /// </param>
        /// <returns>
        /// A <see cref="PIQISAMResponse"/> indicating whether the evaluation passed (<c>true</c>) or failed (<c>false</c>),
        /// or an error if the input was not properly populated.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Thrown if <see cref="PIQISAMRequest.EvaluationObject"/> is not a <see cref="MessageModelItem"/>.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when the message data is null or the <see cref="BaseText.Text"/> is empty.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The evaluation logic:
        /// </para>
        /// <list type="number">
        ///   <item><description>Casts <see cref="PIQISAMRequest.EvaluationObject"/> to <see cref="MessageModelItem"/>.</description></item>
        ///   <item><description>Reads <see cref="MessageModelItem.MessageData"/> as <see cref="BaseText"/>.</description></item>
        ///   <item><description>
        /// Sets <c>passed</c> to <c>true</c> if <paramref name="request"/> data is a <see cref="CodeableConcept"/>.
        /// </description></item>
        ///   <item><description>
        /// Validates that <see cref="BaseText.Text"/> is populated; errors if null/empty.
        /// </description></item>
        ///   <item><description>
        /// Compares the text against a case-insensitive true list: <c>T</c>, <c>True</c>, <c>Y</c>, <c>Yes</c>, <c>1</c>.
        /// </description></item>
        ///   <item><description>
        /// Returns the result via <see cref="PIQISAMResponse.Done(bool)"/> or <see cref="PIQISAMResponse.Error(string)"/>.
        /// </description></item>
        /// </list>
        /// </remarks>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // Set the message model item
                EvaluationItem evaluationItem = (EvaluationItem)request.EvaluationObject;
                MessageModelItem item = evaluationItem?.MessageItem;

                // Evaluate the item's message data
                BaseText data = (BaseText)item.MessageData;

                // Check if the data is a codable concept (initial pass condition)
                passed = (data is CodeableConcept);

                // Verify attribute data
                if (data == null || string.IsNullOrEmpty(data.Text))
                    throw new Exception("Data was unpoulated. Check the sam dependencies");

                // Evaluation lists
                List<string> trueList = new() { "T", "True", "Y", "Yes", "1" };
                List<string> falseList = new() { "F", "False", "N", "No", "0" };

                // For now, the SAM only passes if the data evaluates to TRUE.
                // Commented code would allow skipping when the value is not explicitly FALSE.
                if (trueList.Any(t => t.Equals(data.Text, StringComparison.CurrentCultureIgnoreCase)))
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
        public static string StaticMnemonic => "ATTRIBUTE_INDICATOR_IS_TRUE";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
