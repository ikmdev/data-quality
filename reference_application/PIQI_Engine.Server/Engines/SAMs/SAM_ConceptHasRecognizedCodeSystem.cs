using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if a <see cref="CodeableConcept"/> attribute contains at least one coding
    /// with a recognized code system based on the reference data.
    /// </summary>
    public class SAM_ConceptHasRecognizedCodeSystem : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ConceptHasRecognizedCodeSystem"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ConceptHasRecognizedCodeSystem(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the provided <see cref="MessageModelItem"/>'s <see cref="CodeableConcept"/> contains
        /// at least one coding with a recognized code system according to the provided reference data.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>
        ///     <see cref="PIQISAMRequest.MessageObject"/> – expected to be a <see cref="MessageModelItem"/>
        ///     whose <see cref="MessageModelItem.MessageData"/> is a <see cref="CodeableConcept"/>.
        ///   </item>
        ///   <item>
        ///     Optional entries in <see cref="PIQISAMRequest.ParmList"/> for additional SAM-specific parameters.
        ///   </item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous operation.
        /// The response will indicate:
        /// <list type="bullet">
        ///   <item><c>Done(true)</c> if at least one coding is recognized.</item>
        ///   <item><c>Done(false)</c> if no codings are recognized.</item>
        ///   <item><c>Error</c> if the input is invalid or an exception occurs.</item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">Thrown if the <see cref="MessageModelItem"/> does not contain a <see cref="CodeableConcept"/>.</exception>
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

                // Validate the data type
                if (data is not CodeableConcept)
                    throw new Exception("CodeableConceptCodeSystemRecognized expects a codeable concept value.");

                // Cast data to CodeableConcept
                CodeableConcept codeableConcept = (CodeableConcept)data;

                // Evaluate success if any coding is recognized
                passed = codeableConcept.CodingList.Any(t => t.HasRecognizedCodeSystem);

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
        public static string StaticMnemonic => "CONCEPT_HASRECOGNIZEDCODESYSTEM";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
