using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM (Semantic Assessment Module) that evaluates whether a <see cref="CodeableConcept"/>
    /// contains at least one valid coding according to the FHIR server.
    /// </summary>
    public class SAM_ConceptIsValid : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ConceptIsValid"/> class.
        /// </summary>
        /// <param name="sam">The parent <see cref="SAM"/> object providing configuration and context.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ConceptIsValid(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the provided <see cref="MessageModelItem"/> contains
        /// a <see cref="CodeableConcept"/> with at least one valid coding.
        /// Calls the FHIR $lookup API if needed.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>The <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> containing a <see cref="CodeableConcept"/>.</item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if at least one coding in the <see cref="CodeableConcept"/> is valid.</item>
        ///   <item><c>Failed</c> if no codings are valid.</item>
        ///   <item><c>Errored</c> if the input data is invalid or an exception occurs.</item>
        /// </list>
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the input is not a <see cref="MessageModelItem"/> 
        /// or if its <see cref="MessageModelItem.MessageData"/> is not a <see cref="CodeableConcept"/>.
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

                // Since we're an attr sam we want to play with the item's message data
                BaseText data = (BaseText)item.MessageData;

                // Validate the data format
                if (data is not CodeableConcept codeableConcept)
                    throw new Exception("CodeableConceptIsValidConcept expects a CodeableConcept value.");

                // Call FHIR server if not called already
                if (!codeableConcept.FHIRServerCalled)
                    await _SAMService.LookupCodeAsync(codeableConcept);

                // Check if any codings are valid
                passed = codeableConcept.CodingList.Any(t => t.IsValid);

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
        public static string StaticMnemonic => "CONCEPT_ISVALID";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
