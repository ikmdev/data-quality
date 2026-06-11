using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI.CustomSAMs
{
    /// <summary>
    /// SAM implementation that validates whether the specimen type for lab results
    /// is plausible given the Universal Device Identifier.
    /// </summary>
    public class SAM_SpecimenTypeValidForDevice : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_SpecimenTypeValidForDevice"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_SpecimenTypeValidForDevice(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Gets the static mnemonic for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "SPECIMEN_TYPE_VALID_FOR_DEVICE";

        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;

        /// <summary>
        /// Evaluates whether the specimen type for a lab result is valid for the given device ID.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing the evaluation object.
        /// The evaluation object is expected to be an <see cref="EvaluationItem"/> representing
        /// the Specimen Type attribute.
        /// </param>
        /// <returns>
        /// A <see cref="Task{PIQISAMResponse}"/> representing the asynchronous evaluation result.
        /// </returns>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // Set the evaluation item
                if (request.EvaluationObject is EvaluationItem evaluationItem)
                {
                    // Extract specimen type value from the message data (common name)
                    string? specimenType = (evaluationItem.MessageItem?.MessageData as BaseText)?.Text;

                    // Find the sibling Device ID attribute (LAB_DEVID)
                    var deviceIdItem = _SAMService.Message.EvaluationManager.EvaluationItemDict.Values
                        .FirstOrDefault(t => t.ElementEntityMnemonic == evaluationItem.ElementEntityMnemonic &&
                                             t.ElementSequence == evaluationItem.ElementSequence &&
                                             t.Entity.Mnemonic == "LAB_DEVID");

                    // Extract Device ID value
                    string? deviceId = (deviceIdItem?.MessageItem?.MessageData as BaseText)?.Text;

                    // Evaluate if both Specimen Type and Device ID are present
                    if (!string.IsNullOrEmpty(deviceId) && !string.IsNullOrEmpty(specimenType))
                    {
                        passed = DeviceValidationUtility.IsFuzzyMatch(deviceId, specimenType, "Specimen Concept FQN(s)");
                    }
                }

                // Update result
                result.Done(passed);
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
            }

            return result;
        }
    }
}
