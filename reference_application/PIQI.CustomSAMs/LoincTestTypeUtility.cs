namespace PIQI.CustomSAMs
{
    /// <summary>
    /// Classifies a lab test's expected result nature based on its LOINC code.
    /// </summary>
    public enum LoincTestType
    {
        Quantitative,
        Qualitative
    }

    /// <summary>
    /// Utility class for looking up whether a LOINC code represents a
    /// Quantitative or Qualitative lab test.
    /// </summary>
    public static class LoincTestTypeUtility
    {
        /// <summary>
        /// Returns the test type for a given LOINC code, or null if the code
        /// is not present in the mapping (unknown/unmapped).
        /// </summary>
        /// <param name="loincCode">The LOINC code identifying the lab test.</param>
        /// <returns>The <see cref="LoincTestType"/> for the code, or null if unmapped.</returns>
        public static LoincTestType? GetTestType(string? loincCode)
        {
            if (string.IsNullOrWhiteSpace(loincCode)) return null;

            return loincCode.Trim() switch
            {
                // Verified against LOINC (loinc.org, cross-checked with the NLM Clinical Tables API) on 2026-09-19.
                "2345-7" => LoincTestType.Quantitative,   // Glucose [Mass/volume] in Serum or Plasma
                "5778-6" => LoincTestType.Qualitative,    // Color of Urine
                "2160-0" => LoincTestType.Quantitative,   // Creatinine [Mass/volume] in Serum or Plasma
                "1751-7" => LoincTestType.Quantitative,   // Albumin [Mass/volume] in Serum or Plasma
                "1783-0" => LoincTestType.Quantitative,   // Alkaline phosphatase [Enzymatic activity/volume] in Blood
                "4548-4" => LoincTestType.Quantitative,   // Hemoglobin A1c/Hemoglobin.total in Blood
                "17856-6" => LoincTestType.Quantitative,  // Hemoglobin A1c/Hemoglobin.total in Blood by HPLC (LOINC notes this is not recommended; prefer 4548-4)
                "6768-6" => LoincTestType.Quantitative,   // Alkaline phosphatase [Enzymatic activity/volume] in Serum or Plasma
                "5902-2" => LoincTestType.Quantitative,   // Prothrombin time (PT)
                "6301-6" => LoincTestType.Quantitative,   // INR in Platelet poor plasma by Coagulation assay
                "2161-8" => LoincTestType.Quantitative,   // Creatinine [Mass/volume] in Urine
                "13949-3" => LoincTestType.Qualitative,   // Cytomegalovirus IgG Ab [Presence] in Serum or Plasma by Immunoassay
                "48065-7" => LoincTestType.Quantitative,  // Fibrin D-dimer FEU [Mass/volume] in Platelet poor plasma
                "82176-9" => LoincTestType.Qualitative,   // Respiratory syncytial virus RNA [Presence] in Nasopharynx
                "92977-8" => LoincTestType.Qualitative,   // Influenza virus A RNA [Presence] in Lower respiratory specimen
                "94500-6" => LoincTestType.Qualitative,   // SARS-CoV-2 (COVID-19) RNA [Presence] in Respiratory system specimen
                "92131-2" => LoincTestType.Qualitative,   // Respiratory syncytial virus RNA [Presence] in Respiratory system specimen
                "4996-5" => LoincTestType.Qualitative,    // Cytomegalovirus DNA [Presence] in Blood
                "29604-6" => LoincTestType.Quantitative,  // Cytomegalovirus DNA [#/volume] (viral load) in Blood
                "1748-3" => LoincTestType.Quantitative,   // Albumin [Mass/volume] in Pleural fluid
                "89575-5" => LoincTestType.Qualitative,   // Troponin T.cardiac [Interpretation] in Serum or Plasma Qualitative by High sensitivity method
                "34720-3" => LoincTestType.Quantitative,  // Cytomegalovirus DNA [Units/volume] (viral load) in Specimen
                "2162-6" => LoincTestType.Quantitative,   // Creatinine [Mass/time] in 24 hour Urine
                "10839-9" => LoincTestType.Quantitative,  // Troponin I.cardiac [Mass/volume] in Serum or Plasma
                "12190-5" => LoincTestType.Quantitative,  // Creatinine [Mass/volume] in Body fluid
                "89579-7" => LoincTestType.Quantitative,  // Troponin I.cardiac [Mass/volume] in Serum or Plasma by High sensitivity method
                "91556-1" => LoincTestType.Quantitative,  // Fibrin D-dimer [Mass/volume] in Blood by Immunoassay.DDU
                "34714-6" => LoincTestType.Quantitative,  // INR in Blood by Coagulation assay
                "2862-1" => LoincTestType.Quantitative,   // Albumin [Mass/volume] in Serum or Plasma by Electrophoresis
                "27353-2" => LoincTestType.Quantitative,  // Glucose mean value [Mass/volume] in Blood Estimated from glycated hemoglobin
                "1749-1" => LoincTestType.Quantitative,   // Albumin [Mass/volume] in Peritoneal fluid
                "92976-0" => LoincTestType.Qualitative,   // Influenza virus B RNA [Presence] in Lower respiratory specimen
                "44263-2" => LoincTestType.Quantitative,  // Influenza virus A RNA [Units/volume] (viral load) in Specimen
                "82170-2" => LoincTestType.Qualitative,   // Influenza virus B RNA [Presence] in Nasopharynx
                "92142-9" => LoincTestType.Qualitative,   // Influenza virus A RNA [Presence] in Respiratory system specimen
                "94565-9" => LoincTestType.Qualitative,   // SARS-CoV-2 (COVID-19) RNA [Presence] in Nasopharynx
                "92141-1" => LoincTestType.Qualitative,   // Influenza virus B RNA [Presence] in Respiratory system specimen
                "82169-4" => LoincTestType.Qualitative,   // Influenza virus A H3 RNA [Presence] in Nasopharynx
                "82167-8" => LoincTestType.Qualitative,   // Influenza virus A H1 RNA [Presence] in Nasopharynx
                "111766-2" => LoincTestType.Quantitative, // Fibrin D-dimer [Mass/volume] in Serum, Plasma or Blood by Immunoassay.FEU
                "61151-7" => LoincTestType.Quantitative,  // Albumin [Mass/volume] in Serum or Plasma by Bromocresol green (BCG) dye binding method
                _ => null
            };
        }
    }
}
