using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;

namespace PIQI.CustomSAMs
{
    /// <summary>
    /// SAM implementation that validates whether the LOINC code for lab results
    /// is plausible given the Universal Device Identifier.
    /// </summary>
    public class SAM_LOINCCodeValidForDevice : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_LOINCCodeValidForDevice"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_LOINCCodeValidForDevice(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Gets the static mnemonic for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "LOINC_CODE_VALID_FOR_DEVICE";

        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;

        /// <summary>
        /// Evaluates whether the LOINC code for a lab result is valid for the given device ID.
        /// </summary>
        /// <param name="request">
        /// The <see cref="PIQISAMRequest"/> containing the evaluation object.
        /// The evaluation object is expected to be an <see cref="EvaluationItem"/> representing
        /// the LOINC Code attribute (usually part of a Lab Test CodeableConcept).
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
                    // Access the attribute's message data, which should be a CodeableConcept
                    if (evaluationItem.MessageItem?.MessageData is CodeableConcept codeableConcept)
                    {
                        // Find the sibling Device ID attribute (LAB_DEVID)
                        var deviceIdItem = _SAMService.Message.EvaluationManager.EvaluationItemDict.Values
                            .FirstOrDefault(t => t.ElementEntityMnemonic == evaluationItem.ElementEntityMnemonic &&
                                                 t.ElementSequence == evaluationItem.ElementSequence &&
                                                 t.Entity.Mnemonic == "LAB_DEVID");

                        // Extract Device ID value
                        string? deviceId = (deviceIdItem?.MessageItem?.MessageData as BaseText)?.Text;

                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            // Extract LOINC codes from the CodeableConcept
                            var loincCodes = codeableConcept.CodingList
                                .Where(c => c.CodeSystem == "2.16.840.1.113883.6.1" || c.CodeSystem == "http://loinc.org" || c.CodeSystem == "REGEN_LOINC")
                                .Select(c => c.CodeValue)
                                .Where(v => !string.IsNullOrEmpty(v))
                                .ToList();

                            // If no explicit LOINC system match, we could optionally check all codings
                            // but usually it's better to be specific if the system is known.
                            // For this implementation, we'll check any LOINC codes found.
                            foreach (var loincCode in loincCodes)
                            {
                                if (DeviceValidationUtility.IsDirectMatch(deviceId, loincCode, DeviceTableColumn.LoincCodeIdentifiers))
                                {
                                    passed = true;
                                    break;
                                }
                            }
                        }
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
