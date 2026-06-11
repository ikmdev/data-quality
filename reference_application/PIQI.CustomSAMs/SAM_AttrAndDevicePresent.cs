using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI.CustomSAMs
{
    /// <summary>
    /// SAM implementation that validates both the entity to test and the device ID are present.
    /// This is used as a prerequisite for other device-specific validations.
    /// </summary>
    public class SAM_AttrAndDevicePresent : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_AttrAndDevicePresent"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_AttrAndDevicePresent(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Gets the static mnemonic for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "ATTR_AND_DEVICE_PRESENT";

        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;

        /// <summary>
        /// Evaluates whether both the triggering attribute and its sibling Device ID are present and populated.
        /// </summary>
        /// <param name="request">The evaluation request containing the attribute to test.</param>
        /// <returns>A result indicating passed if both are present, or skipped if either is missing.</returns>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();
            bool passed = false;

            try
            {
                // Set the evaluation item
                if (request.EvaluationObject is EvaluationItem evaluationItem)
                {
                    // Find the sibling Device ID attribute (LAB_DEVID)
                    // Attributes in the same element instance share the same ElementEntityMnemonic and ElementSequence
                    var deviceIdItem = _SAMService.Message.EvaluationManager.EvaluationItemDict.Values
                        .FirstOrDefault(t => t.ElementEntityMnemonic == evaluationItem.ElementEntityMnemonic &&
                                             t.ElementSequence == evaluationItem.ElementSequence &&
                                             t.Entity.Mnemonic == "LAB_DEVID");

                    if (deviceIdItem?.MessageItem?.MessageData is BaseText data)
                    {
                        // Check if the Device ID is populated
                        passed = !string.IsNullOrEmpty(data.Text);
                    }
                }

                // Update result
                result.Done(passed);
            }
            catch (Exception ex)
            {
                result.Error(ex.ToString());
            }

            return result;
        }
    }
}
