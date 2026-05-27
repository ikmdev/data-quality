using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if a <see cref="CodeableConcept"/> contains at least one complete coding.
    /// </summary>
    public class SAM_ConceptIsComplete : SAMBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ConceptIsComplete"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ConceptIsComplete(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the specified <see cref="MessageModelItem"/>'s <see cref="CodeableConcept"/> contains
        /// at least one complete coding.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> whose <see cref="MessageModelItem.MessageData"/> is a <see cref="CodeableConcept"/>.</item>
        /// </list>
        /// Optional parameters can be passed but are not used in this evaluation.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if at least one coding is complete.</item>
        ///   <item><c>Failed</c> if no codings are complete.</item>
        ///   <item><c>Errored</c> if the input data is invalid or an exception occurs.</item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">Thrown if the provided attribute is not a <see cref="CodeableConcept"/>.</exception>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // First param is always a message model item
                EvaluationItem evaluationItem = (EvaluationItem)request.EvaluationObject;
                MessageModelItem item = evaluationItem?.MessageItem;

                // Access the attribute's message data
                BaseText data = (BaseText)item.MessageData;

                // Validate the data format
                if (data is not CodeableConcept)
                    throw new Exception("CodeableConceptIsComplete expects a codeable concept value.");

                // Cast data as CodeableConcept
                CodeableConcept codeableConcept = (CodeableConcept)data;

                // Evaluate: check if any coding is complete
                passed = codeableConcept.CodingList.Any(t => t.IsComplete);

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
        public static string StaticMnemonic => "CONCEPT_ISCOMPLETE";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
