using PIQI.Components.SAMs;
using PIQI.Components.Models;
using PIQI.Components.Services;
using Serilog;

namespace PIQI.CustomSAMs
{
    /// <summary>
    /// SAM implementation that validates a lab result value against the rules
    /// appropriate for its test's Quantitative/Qualitative nature, as classified
    /// by the test's LOINC code via <see cref="LoincTestTypeUtility"/>.
    /// </summary>
    public class SAM_ResultValueValidForTestType : SAMBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAM_ResultValueValidForTestType"/> class.
        /// </summary>
        /// <param name="sam">The SAM object associated with this evaluator.</param>
        /// <param name="samService">
        /// An implementation of <see cref="SAMService"/> used to access reference data and make FHIR API calls.
        /// </param>
        public SAM_ResultValueValidForTestType(SAM sam, SAMService samService) : base(sam, samService) { }

        /// <summary>
        /// Gets the static mnemonic for this SAM implementation.
        /// </summary>
        public static string StaticMnemonic => "RESULT_VALUE_VALID_FOR_TEST_TYPE";

        /// <summary>
        /// Gets the mnemonic string associated with this instance.
        /// </summary>
        public override string Mnemonic => StaticMnemonic;

        /// <summary>
        /// Evaluates whether a lab result value is valid for the Quantitative or Qualitative
        /// nature of its test:
        /// <list type="bullet">
        /// <item>Quantitative: the result value and the reference range low/high values must all be numeric, and high must be greater than low.</item>
        /// <item>Qualitative: the result value must be non-numeric, the reference range must be blank, and the result value must be an acceptable value from the ResVal value list.</item>
        /// </list>
        /// </summary>
        public override async Task<PIQISAMResponse> EvaluateAsync(PIQISAMRequest request)
        {
            PIQISAMResponse result = new();

            try
            {
                if (request.EvaluationObject is not EvaluationItem evaluationItem)
                {
                    result.Error("Evaluation object was not an EvaluationItem.");
                    return result;
                }

                // LAB_RESVALUE's own message data
                if (evaluationItem.MessageItem?.MessageData is not BaseText resultData || string.IsNullOrEmpty(resultData.Text))
                    return result.Fail("Result value attribute was unpopulated or not text-based.");

                string resultText = resultData.Text;

                // Find the sibling Lab Test attribute (LAB_TEST), which carries the LOINC code
                var testItem = _SAMService.Message.EvaluationManager.EvaluationItemDict.Values
                    .FirstOrDefault(t => t.ElementEntityMnemonic == evaluationItem.ElementEntityMnemonic &&
                                         t.ElementSequence == evaluationItem.ElementSequence &&
                                         t.Entity.Mnemonic == "LAB_TEST");

                string? loincCode = null;
                if (testItem?.MessageItem?.MessageData is CodeableConcept testConcept)
                {
                    loincCode = testConcept.CodingList
                        .Where(c => c.CodeSystem == "2.16.840.1.113883.6.1" || c.CodeSystem == "http://loinc.org" || c.CodeSystem == "REGEN_LOINC")
                        .Select(c => c.CodeValue)
                        .FirstOrDefault(v => !string.IsNullOrEmpty(v));
                }

                if (string.IsNullOrEmpty(loincCode))
                    return result.Fail("Unable to determine LOINC code for the lab test; cannot validate result value.");

                LoincTestType? testType = LoincTestTypeUtility.GetTestType(loincCode);
                if (testType == null)
                {
                    Log.Warning("LOINC code {LoincCode} is not mapped in LoincTestTypeUtility; result value could not be validated. Add a mapping entry to remediate.", loincCode);
                    return result.Fail($"LOINC code [{loincCode}] test-type (i.e., Qual / Quant) could not be determined.");
                }

                // Find the sibling Reference Range attribute (LAB_REFRNG)
                var refRangeItem = _SAMService.Message.EvaluationManager.EvaluationItemDict.Values
                    .FirstOrDefault(t => t.ElementEntityMnemonic == evaluationItem.ElementEntityMnemonic &&
                                         t.ElementSequence == evaluationItem.ElementSequence &&
                                         t.Entity.Mnemonic == "LAB_REFRNG");

                ReferenceRange? refRange = refRangeItem?.MessageItem?.MessageData as ReferenceRange;

                (bool passed, string? failReason) = testType == LoincTestType.Quantitative
                    ? EvaluateQuantitative(resultText, refRange)
                    : EvaluateQualitative(resultText, refRange);

                if (!passed) return result.Fail(failReason);
                result.Done(true);
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Validates a quantitative result: the result value and the reference range's
        /// low/high values must all be numeric, and high must be greater than low.
        /// </summary>
        private static (bool passed, string? failReason) EvaluateQuantitative(string resultText, ReferenceRange? refRange)
        {
            if (!double.TryParse(resultText, out double _))
                return (false, "Quantitative test result value is not numeric.");

            if (refRange == null)
                return (false, "Quantitative test is missing a reference range.");

            if (!double.TryParse(refRange.LowValue, out double low))
                return (false, "Reference range low value is missing or not numeric.");

            if (!double.TryParse(refRange.HighValue, out double high))
                return (false, "Reference range high value is missing or not numeric.");

            if (high <= low)
                return (false, "Reference range high value must be greater than the low value.");

            return (true, null);
        }

        /// <summary>
        /// Validates a qualitative result: the result value must be non-numeric, the
        /// reference range must be blank, and the result value must be an acceptable
        /// value from the ResVal value list.
        /// </summary>
        private (bool passed, string? failReason) EvaluateQualitative(string resultText, ReferenceRange? refRange)
        {
            if (double.TryParse(resultText, out double _))
                return (false, "Qualitative test result value must not be numeric.");

            if (refRange != null && (refRange.HasLow || refRange.HasHigh))
                return (false, "Qualitative test must not have reference range low/high values populated.");

            if (_SAMService.Message.RefData?.ValueList == null)
                throw new Exception("Missing or invalid reference data for SAM_ResultValueValidForTestType.");

            ValueList? resVal = _SAMService.Message.RefData.ValueList
                .FirstOrDefault(v => v.Mnemonic.Equals("ResVal", StringComparison.OrdinalIgnoreCase));

            if (resVal == null)
                throw new Exception("Value data [ResVal] not in RefData. Check processing engine.");

            bool inList = resVal.CodeList.Any(c =>
                c.DataCode.Equals(resultText, StringComparison.OrdinalIgnoreCase) ||
                c.DataText.Equals(resultText, StringComparison.OrdinalIgnoreCase));

            if (!inList)
                return (false, $"Qualitative result value [{resultText}] is not an acceptable value.");

            return (true, null);
        }
    }
}
