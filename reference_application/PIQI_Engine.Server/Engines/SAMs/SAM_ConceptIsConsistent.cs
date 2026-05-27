using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI_Engine.Server.Engines.SAMs
{
    /// <summary>
    /// SAM implementation that checks if a <see cref="CodeableConcept"/> is semantically consistent.
    /// </summary>
    public class SAM_ConceptIsConsistent : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ConceptIsConsistent"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ConceptIsConsistent(SAM sam, SAMService samService)
            : base(sam, samService) { }

        /// <summary>
        /// Evaluates whether the specified <see cref="MessageModelItem"/>'s <see cref="CodeableConcept"/> 
        /// is semantically consistent by comparing its coding display text to reference display values.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing:
        /// <list type="bullet">
        ///   <item>A <see cref="PIQISAMRequest.MessageObject"/>, expected to be a <see cref="MessageModelItem"/> with a <see cref="CodeableConcept"/> as its <see cref="MessageModelItem.MessageData"/>.</item>
        ///   <item>Optional entries in <see cref="PIQISAMRequest.ParmList"/>, where one parameter can specify the semantic similarity threshold (integer, default 50).</item>
        /// </list>
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// The response indicates:
        /// <list type="bullet">
        ///   <item><c>Succeeded</c> if at least one coding is semantically consistent with a reference display.</item>
        ///   <item><c>Failed</c> if no codings meet the semantic consistency threshold.</item>
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
                    throw new Exception("CodeableConceptIsSemanticallyConsistent expects a codeable concept value.");

                // Cast data as CodeableConcept
                CodeableConcept codeableConcept = (CodeableConcept)data;

                // Populate reference display list via FHIR $lookup if not already done
                if (!codeableConcept.FHIRServerCalled)
                    await _SAMService.LookupCodeAsync(codeableConcept);

                // Get threshold
                int threshold = 50;

                // Compute semantic consistency using Levenshtein distance
                foreach (Coding coding in codeableConcept.CodingList.Where(t => t.IsValid))
                {
                    int bestScore = 0;
                    foreach (string referenceDisplay in coding.ReferenceDisplayList)
                    {
                        int ld = Utility.ComputeLevenshteinDistance(coding.CodeText.ToUpper(), referenceDisplay.ToUpper());
                        int maxLen = Math.Max(coding.CodeText.Length, referenceDisplay.Length);
                        int score = 100 - (int)((double)ld / maxLen * 100);
                        if (score > bestScore) bestScore = score;
                    }
                    coding.IsSemantic = (bestScore >= threshold);
                }

                // Evaluate
                passed = codeableConcept.CodingList.Any(t => t.IsSemantic);

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
        public static string StaticMnemonic => "CONCEPT_ISCONSISTENT";
        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;
    }
}
