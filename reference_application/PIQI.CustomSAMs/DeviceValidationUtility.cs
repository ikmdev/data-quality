using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PIQI.CustomSAMs
{
    /// <summary>
    /// Represents the columns available in the device mapping table.
    /// </summary>
    public enum DeviceTableColumn
    {
        DeviceName,
        DeviceIdentifier,
        DeviceConceptUUID,
        TestPerformedSemanticUUID,
        LoincCodeIdentifiers,
        InstrumentConceptGlobalStandardsOneIdentifiers,
        SpecimenConceptFQN,
        UnitsOfMeasure,
        PopulationReferenceRangeSemanticUUIDs,
        QuantitativeAllowedResultsRangePatternUUIDs
    }

    /// <summary>
    /// Utility class for validating device-specific attributes against a mapping table.
    /// </summary>
    public static class DeviceValidationUtility
    {
        /// <summary>
        /// Maps the <see cref="DeviceTableColumn"/> enum to the exact header strings in the mapping table.
        /// </summary>
        private static readonly Dictionary<DeviceTableColumn, string> ColumnNames = new Dictionary<DeviceTableColumn, string>
        {
            { DeviceTableColumn.DeviceName, "Device Name" },
            { DeviceTableColumn.DeviceIdentifier, "Device Identifier" },
            { DeviceTableColumn.DeviceConceptUUID, "Device Concept UUID" },
            { DeviceTableColumn.TestPerformedSemanticUUID, "Test Performed Semantic UUID" },
            { DeviceTableColumn.LoincCodeIdentifiers, "LOINC Code Identifier(s)" },
            { DeviceTableColumn.InstrumentConceptGlobalStandardsOneIdentifiers, "Instrument Concept Global Standards One Identifier(s)" },
            { DeviceTableColumn.SpecimenConceptFQN, "Specimen Concept FQN(s)" },
            { DeviceTableColumn.UnitsOfMeasure, "Units of Measure" },
            { DeviceTableColumn.PopulationReferenceRangeSemanticUUIDs, "Population Reference Range Semantic UUID(s)" },
            { DeviceTableColumn.QuantitativeAllowedResultsRangePatternUUIDs, "Quantitative Allowed Results Range Pattern UUID(s)" }
        };


        /// <summary>
        /// Retrieves the mapping table string for a specific device.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <returns>The mapping table string if found; otherwise, null.</returns>
        public static string GetTableForDevice(string deviceId)
        {
            return deviceId switch
            {
                "08426950060451" => DEVICE_08426950060451,
                "00380740175498" => DEVICE_00380740175498,
                "00380740135164" => DEVICE_00380740135164,
                "00380740135591" => DEVICE_00380740135591,
                "00815381020529" => DEVICE_00815381020529,
                "03607360025031" => DEVICE_03607360025031,
                "00380740159528" => DEVICE_00380740159528,
                "08426950485605" => DEVICE_08426950485605,
                "08426950447627" => DEVICE_08426950447627,
                "08056771101905" => DEVICE_08056771101905,
                "04560189282919" => DEVICE_04560189282919,
                "20758750008343" => DEVICE_20758750008343,
                "10758750002849" => DEVICE_10758750002849,
                "20758750002501" => DEVICE_20758750002501,
                "10758750004522" => DEVICE_10758750004522,
                "07332940008604" => DEVICE_07332940008604,
                "07332940008581" => DEVICE_07332940008581,
                "10054749000187" => DEVICE_10054749000187,
                _ => null
            };
        }

        /// <summary>
        /// Retrieves all unique values from a specific column in the device mapping table.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="columnName">The name of the column to retrieve values from.</param>
        /// <returns>A list of string values from the specified column.</returns>
        public static List<string> GetColumnValues(string deviceId, string columnName)
        {
            var results = new List<string>();
            string table = GetTableForDevice(deviceId);
            if (string.IsNullOrEmpty(table)) return results;

            var rows = table.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length < 3) return results;

            var header = rows[0].Split('|', StringSplitOptions.TrimEntries);
            int columnIndex = Array.IndexOf(header, columnName);
            if (columnIndex == -1) return results;

            foreach (var row in rows.Skip(2))
            {
                var cols = row.Split('|', StringSplitOptions.TrimEntries);
                if (cols.Length > columnIndex) results.Add(cols[columnIndex]);
            }
            return results;
        }

        /// <summary>
        /// Retrieves all unique values from a specific column in the device mapping table.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="column">The column enum to retrieve values from.</param>
        /// <returns>A list of string values from the specified column.</returns>
        public static List<string> GetColumnValues(string deviceId, DeviceTableColumn column)
            => GetColumnValues(deviceId, ColumnNames[column]);

        /// <summary>
        /// Retrieves all clean individual values from a specific column in the device mapping table,
        /// parsing HTML lists into separate items.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="columnName">The name of the column to retrieve values from.</param>
        /// <returns>A flat list of parsed string values from the specified column.</returns>
        public static List<string> GetColumnValuesClean(string deviceId, string columnName)
        {
            var rawValues = GetColumnValues(deviceId, columnName);
            var results = new List<string>();
            foreach (var val in rawValues)
            {
                results.AddRange(ParseHtmlListItems(val));
            }
            return results;
        }

        /// <summary>
        /// Retrieves all clean individual values from a specific column in the device mapping table,
        /// parsing HTML lists into separate items.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="column">The column enum to retrieve values from.</param>
        /// <returns>A flat list of parsed string values from the specified column.</returns>
        public static List<string> GetColumnValuesClean(string deviceId, DeviceTableColumn column)
            => GetColumnValuesClean(deviceId, ColumnNames[column]);

        /// <summary>
        /// Performs an exact, case-insensitive match for a value against a specific column in the device table.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="columnName">The name of the column to check against.</param>
        /// <returns>True if an exact match is found; otherwise, false.</returns>
        public static bool IsDirectMatch(string deviceId, string value, string columnName)
        {
            return AnyMatch(deviceId, columnName, 
                v => string.Equals(v, value, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Performs an exact, case-insensitive match for a value against a specific column in the device table.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="column">The column enum to check against.</param>
        /// <returns>True if an exact match is found; otherwise, false.</returns>
        public static bool IsDirectMatch(string deviceId, string value, DeviceTableColumn column)
            => IsDirectMatch(deviceId, value, ColumnNames[column]);

        /// <summary>
        /// Performs a substring match (contains) for a value against a specific column in the device table.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="columnName">The name of the column to check against.</param>
        /// <returns>True if the column value contains the test value; otherwise, false.</returns>
        public static bool IsFuzzyMatch(string deviceId, string value, string columnName)
        {
            return AnyMatch(deviceId, columnName, 
                v => v.Contains(value, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Performs a substring match (contains) for a value against a specific column in the device table.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="column">The column enum to check against.</param>
        /// <returns>True if the column value contains the test value; otherwise, false.</returns>
        public static bool IsFuzzyMatch(string deviceId, string value, DeviceTableColumn column)
            => IsFuzzyMatch(deviceId, value, ColumnNames[column]);
        
        /// <summary>
        /// Parses an HTML unordered list string into a list of individual items.
        /// Extracts top-level list items and strips HTML tags.
        /// </summary>
        /// <param name="cellValue">The raw cell value potentially containing an HTML list.</param>
        /// <returns>A list of clean string items.</returns>
        private static List<string> ParseHtmlListItems(string cellValue)
        {
            if (string.IsNullOrWhiteSpace(cellValue))
                return new List<string>();

            // If no list items, treat as plain text
            if (cellValue.IndexOf("<li>", StringComparison.OrdinalIgnoreCase) == -1)
            {
                var trimmed = cellValue.Trim();
                return string.IsNullOrEmpty(trimmed) ? new List<string>() : new List<string> { trimmed };
            }

            var results = new List<string>();
            // Capture everything inside <li> up to the first <ul> or the closing </li>
            var liRegex = new Regex(@"<li>(?<content>(?:(?!<ul|</li>).)*).*?</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var tagRegex = new Regex(@"<[^>]*>", RegexOptions.Singleline);

            var matches = liRegex.Matches(cellValue);
            foreach (Match match in matches)
            {
                string content = match.Groups["content"].Value;
                string cleaned = tagRegex.Replace(content, "").Trim();
                if (!string.IsNullOrEmpty(cleaned))
                {
                    results.Add(cleaned);
                }
            }

            return results;
        }

        /// <summary>
        /// Checks if any row in the device table matches the provided predicate for a specific column.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <param name="columnName">The name of the column to check.</param>
        /// <param name="predicate">The logic to use for matching.</param>
        /// <returns>True if any match is found; otherwise, false.</returns>
        private static bool AnyMatch(string deviceId, string columnName, Func<string, bool> predicate)
        {
            string table = GetTableForDevice(deviceId);
            if (string.IsNullOrEmpty(table)) return false;

            var rows = table.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (rows.Length < 3) return false;

            var header = rows[0].Split('|', StringSplitOptions.TrimEntries);
            int columnIndex = Array.IndexOf(header, columnName);
            if (columnIndex == -1) return false;

            foreach (var row in rows.Skip(2))
            {
                var cols = row.Split('|', StringSplitOptions.TrimEntries);
                if (cols.Length > columnIndex)
                {
                    var items = ParseHtmlListItems(cols[columnIndex]);
                    if (items.Any(predicate)) return true;
                }
            }
            return false;
        }

        private static readonly string DEVICE_08426950060451 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ALKP | 08426950060451 | 688c9852-6f7f-58cd-a511-997184a1add4 | b03595aa-8082-4844-93a5-25a6c226b6e5 | <ul><li>6768-6</li></ul> | <ul><li>00380740137380</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | U/L |  | <ul><li>190d92e7-0aac-4c55-81eb-402e2c7236c6<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 2.1</li><li>Allowable Range Minimum Value: 0.6</li><li>Example UCUM Units: ukat/L</li></ul></li></ul> |

";

        private static readonly string DEVICE_00380740175498 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ARCHITECT STAT High Sensitivity Troponin-I Reagent Kit 100 Tests | 00380740175498 | c7e7a0d3-5e64-5f95-9b20-67b6a2e59f14 | 138e50c6-4246-4269-85f1-2e6e86195078 | <ul><li>89579-7</li></ul> | <ul><li>06438153000099</li></ul> | <ul><li>Plasma specimen (specimen)</li></ul> | ng/L | <ul><li>de62fc89-d659-4c95-8bf5-0169b1ad4dcf<ul><li>Reference Range Population: Female</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Less than or equal to</li><li>Maximum Value; Max Value: 17.0</li><li>Minimum Value; Min Value: 17.0</li><li>Example UCUM Units: ng/L</li></ul></li><li>f83d64cd-d7bd-4c2c-8dd9-75cf49bbf560<ul><li>Reference Range Population: Male</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Less than or equal to</li><li>Maximum Value; Max Value: 35.0</li><li>Minimum Value; Min Value: 35.0</li><li>Example UCUM Units: ng/L</li></ul></li><li>b32d09b5-66bc-48bb-a3d2-2be0db390eba<ul><li>Reference Range Population: Overall</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Less than or equal to</li><li>Maximum Value; Max Value: 28.0</li><li>Minimum Value; Min Value: 28.0</li><li>Example UCUM Units: ng/L</li></ul></li></ul> |  |

";

        private static readonly string DEVICE_00380740135164 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Alinity c Alkaline Phosphatase Reagent Kit 4000 Tests | 00380740135164 | 8dd76bce-4b40-548b-9188-a47f5fe9c7b3 | b03595aa-8082-4844-93a5-25a6c226b6e5 | <ul><li>6768-6</li></ul> | <ul><li>00380740137380</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | U/L |  | <ul><li>190d92e7-0aac-4c55-81eb-402e2c7236c6<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 2.1</li><li>Allowable Range Minimum Value: 0.6</li><li>Example UCUM Units: ukat/L</li></ul></li></ul> |

";

        private static readonly string DEVICE_00380740135591 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Alinity c Hemoglobin A1c Reagent Kit 1300 Tests | 00380740135591 | c000a2fb-47b2-5900-9bd5-2c1661e4daff | f1a038cf-f5ed-49d7-ab23-64f04f0284ab | <ul><li>4548-4</li><li>17855-8</li></ul> | <ul><li>00380740137380</li></ul> | <ul><li>Whole blood specimen (specimen)</li></ul> | % | <ul><li>b739f833-d9c9-44a4-b0d1-573725a91bb9<ul><li>Reference Range Population: At risk of developing diabetes</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 6.4</li><li>Minimum Value; Min Value: 5.7</li><li>Example UCUM Units: %</li></ul></li><li>f0ebb5ec-5323-4d06-b6a5-5feaf3648403<ul><li>Reference Range Population: Less Stringent</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 8.0</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: %</li></ul></li><li>544b7270-8901-49c7-98f2-5fdec98303fb<ul><li>Reference Range Population: General (Non-Pregnant Adults)</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 7.0</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: %</li></ul></li><li>e93bee93-70b6-4759-b8c8-0e6844b18214<ul><li>Reference Range Population: More Stringent</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 6.5</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: %</li></ul></li></ul> |  |
| Alinity c Hemoglobin A1c Reagent Kit 1300 Tests | 00380740135591 | c000a2fb-47b2-5900-9bd5-2c1661e4daff | 7dc8892e-246e-4052-8ad7-274c3be4ad4f | <ul><li>17855-8</li><li>4548-4</li></ul> | <ul><li>00380740137380</li></ul> | <ul><li>Whole blood specimen (specimen)</li></ul> | mmol/mol | <ul><li>ca0e7d5c-ece3-43dc-89e7-24876bd47c79<ul><li>Reference Range Population: Less Stringent</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 64.0</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: mmol/mol</li></ul></li><li>74192c04-94de-430f-a0bc-d278479b3ec7<ul><li>Reference Range Population: At risk of developing diabetes</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 46.0</li><li>Minimum Value; Min Value: 39.0</li><li>Example UCUM Units: mmol/mol</li></ul></li><li>28b17f87-02cd-4b98-a2ce-40a5dc90ee2e<ul><li>Reference Range Population: More Stringent</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 48.0</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: mmol/mol</li></ul></li><li>3512281c-21a9-4076-b0f7-61ed764254fe<ul><li>Reference Range Population: General (Non-Pregnant Adults)</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 53.0</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: mmol/mol</li></ul></li></ul> |  |

";

        private static readonly string DEVICE_00815381020529 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 58771518-6a40-48cb-bd37-e2a1e92153b3 | <ul><li>82160-3</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 21b21ae4-bba3-4414-bee8-e59345857676 | <ul><li>82163-7</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | TCID50/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 5efd9a90-3bcc-49cd-8148-92f4c37df207 | <ul><li>82161-1</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | d644e077-3607-460d-9a69-7bb08e828f83 | <ul><li>82162-9</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | TCID50/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 1fd3bbc3-1afd-48aa-bcf7-ea26cd87e4a2 | <ul><li>82164-5</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | TCID50/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 5b99b8f4-4d37-448a-996c-219717f89639 | <ul><li>94565-9</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Swab of internal nose (specimen)</li><li>Nasopharyngeal swab (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 256985c4-6c9e-4e50-b5f6-bccc1a4ef016 | <ul><li>82165-2</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | TCID50/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 0dd0ccf6-b96a-4294-aba3-e519f98ce2d0 | <ul><li>82175-1</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 4637b67f-efc6-4c19-a2bf-9954b2f04236 | <ul><li>82166-0</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | fed165ab-d0af-4920-929d-952cdcfe82d3 | <ul><li>82170-2</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | e33f1da2-d7a1-416e-bae9-e553e285b3f1 | <ul><li>82171-0</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | a8a8cd23-7330-4f92-8c1e-e9fca9dc7bf5 | <ul><li>82172-8</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | abc5763d-5411-4c7b-81b0-0a197d46cdf1 | <ul><li>82173-6</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 0728e11a-493d-4409-ae2e-9cbb4dc7b310 | <ul><li>82174-4</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 176ee7c5-7845-48da-b0b4-b3d90e406233 | <ul><li>82176-9</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 54dbf696-873b-4ab4-8d7e-39760417a3f9 | <ul><li>87621-9</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 6e366080-4c57-4129-b64e-260413488275 | <ul><li>101284-8</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | CFU/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 06d84763-830e-487d-b723-20b6f238ef2a | <ul><li>82178-5</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |
| BioFire® Respiratory Panel 2.1 | 00815381020529 | f68ce1c1-6a45-5908-9010-55e4837d41c8 | 372d6744-300e-4bce-b467-79efbeba3fc8 | <ul><li>82177-7</li></ul> | <ul><li>00815381020277</li><li>00815381020031</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | copies/mL |  |  |

";

        private static readonly string DEVICE_03607360025031 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| CAPI 3 PROTEIN(E) 6 | 03607360025031 | 5b660e58-0693-53f4-8332-96e107b7b299 | 3dc9ec6e-30e6-4add-b586-196c30243bfd | <ul><li>2862-1</li><li>6942-7</li></ul> | <ul><li>03607360012444</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Urine specimen (specimen)</li></ul> | mg/dL |  | <ul><li>0fc75b00-c036-42c5-bd23-c6399d128968<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 66.1</li><li>Allowable Range Minimum Value: 55.8</li><li>Example UCUM Units: %</li></ul></li></ul> |
| CAPI 3 PROTEIN(E) 6 | 03607360025031 | 5b660e58-0693-53f4-8332-96e107b7b299 | dc9d6e3e-2bda-41f3-97e1-e03cd623f403 | <ul><li>9734-5</li><li>2865-4</li></ul> | <ul><li>03607360012444</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Urine specimen (specimen)</li></ul> | mg/dL |  | <ul><li>fd433478-3174-4fac-9ae7-abf0601b283f<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 4.9</li><li>Allowable Range Minimum Value: 2.9</li><li>Example UCUM Units: %</li></ul></li></ul> |
| CAPI 3 PROTEIN(E) 6 | 03607360025031 | 5b660e58-0693-53f4-8332-96e107b7b299 | 88fa21cd-95c9-4110-ad52-708fd70a671c | <ul><li>38190-5</li><li>2868-8</li></ul> | <ul><li>03607360012444</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Urine specimen (specimen)</li></ul> | mg/dL |  | <ul><li>99ebad53-7c5f-46ff-8a5c-dbd85d75d616<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 11.8</li><li>Allowable Range Minimum Value: 7.1</li><li>Example UCUM Units: %</li></ul></li></ul> |
| CAPI 3 PROTEIN(E) 6 | 03607360025031 | 5b660e58-0693-53f4-8332-96e107b7b299 | 169042e6-ab7e-49ec-8c23-cb5ef7f31981 | <ul><li>54353-8</li><li>32730-4</li></ul> | <ul><li>03607360012444</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Urine specimen (specimen)</li></ul> | mg/dL |  | <ul><li>0774e6f2-4871-4da8-b4c0-a99413bf4e37<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 7.2</li><li>Allowable Range Minimum Value: 4.7</li><li>Example UCUM Units: %</li></ul></li></ul> |
| CAPI 3 PROTEIN(E) 6 | 03607360025031 | 5b660e58-0693-53f4-8332-96e107b7b299 | ab6acf6e-5c8d-40da-bb54-b5be482cd398 | <ul><li>54354-6</li><li>32731-2</li></ul> | <ul><li>03607360012444</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Urine specimen (specimen)</li></ul> | mg/dL |  | <ul><li>c6aa82dc-8deb-4a06-84f2-eba54d41b504<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 6.5</li><li>Allowable Range Minimum Value: 3.2</li><li>Example UCUM Units: %</li></ul></li></ul> |
| CAPI 3 PROTEIN(E) 6 | 03607360025031 | 5b660e58-0693-53f4-8332-96e107b7b299 | 23e55d31-6ee1-405f-ac86-ede2a86f90d9 | <ul><li>9745-1</li><li>2874-6</li></ul> | <ul><li>03607360012444</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Urine specimen (specimen)</li></ul> | mg/dL |  | <ul><li>38940952-c323-4342-8d4b-2ec6e591463b<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 18.8</li><li>Allowable Range Minimum Value: 11.1</li><li>Example UCUM Units: %</li></ul></li></ul> |

";

        private static readonly string DEVICE_00380740159528 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Creatinine2 4500 Tests | 00380740159528 | c4e3125d-d3a0-5e8a-b7fd-ffa9701d7c9d | 90a69aeb-70df-4210-978f-59f08c81bb82 | <ul><li>2160-0</li></ul> | <ul><li>00380740137380</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | mg/dL | <ul><li>38841b66-205e-429a-b5e3-54f9e5775ecb<ul><li>Reference Range Population: Female</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 1.04</li><li>Minimum Value; Min Value: 0.52</li><li>Example UCUM Units: mg/dL</li></ul></li><li>3905ceca-60fa-4513-b9ae-0210362f84ab<ul><li>Reference Range Population: Female</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 1.04</li><li>Minimum Value; Min Value: 0.52</li><li>Example UCUM Units: mg/mL</li></ul></li></ul> |  |
| Creatinine2 4500 Tests | 00380740159528 | c4e3125d-d3a0-5e8a-b7fd-ffa9701d7c9d | afe8d979-bb66-4c26-88ba-3df5f9b854c7 | <ul><li>2161-8</li></ul> | <ul><li>00380740137380</li></ul> | <ul><li>Urine specimen (specimen)</li></ul> | mg/dL | <ul><li>2fbee6c2-6f7b-4aed-bc21-3f69f342b2b4<ul><li>Reference Range Population: Male < 40 years</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 392.0</li><li>Minimum Value; Min Value: 24.0</li><li>Example UCUM Units: mg/dL</li></ul></li><li>1b19861f-cad2-4de8-af04-df00a0b1defa<ul><li>Reference Range Population: Male ≥ 40 years</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 328.0</li><li>Minimum Value; Min Value: 22.0</li><li>Example UCUM Units: mg/dL</li></ul></li><li>0baab9f7-d8ed-4d14-a1fb-6373c1ebd37f<ul><li>Reference Range Population: Female < 40 years</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 327.0</li><li>Minimum Value; Min Value: 16.0</li><li>Example UCUM Units: mg/dL</li></ul></li><li>bad2ac50-318e-4f26-9304-fbb2f1907e99<ul><li>Reference Range Population: Female ≥ 40 years</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 278.0</li><li>Minimum Value; Min Value: 15.0</li><li>Example UCUM Units: mg/dL</li></ul></li></ul> |  |

";

        private static readonly string DEVICE_08426950485605 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| HemosIL D-Dimer HS 500 | 08426950485605 | 3be68d35-c40a-5708-ba68-a38a9f6f2c63 | 48ece425-e8ab-444e-ac81-15e56b4b1c0e | <ul><li>6768-6</li></ul> | <ul><li>10758750031610</li><li>10758750012343</li><li>10758750002740</li><li>10758750001330</li><li>10758750002054</li><li>10758750031986</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | U/L, ukat/L |  | <ul><li>451b47b3-6c1e-407c-be45-f68d4f7d0716<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 126.0</li><li>Allowable Range Minimum Value: 38.0</li><li>Example UCUM Units: U/L</li></ul></li><li>190d92e7-0aac-4c55-81eb-402e2c7236c6<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 2.1</li><li>Allowable Range Minimum Value: 0.6</li><li>Example UCUM Units: ukat/L</li></ul></li></ul> |
| HemosIL D-Dimer HS 500 | 08426950485605 | 3be68d35-c40a-5708-ba68-a38a9f6f2c63 | 9ab8cc26-94d7-4730-8905-9acc8148b11d |  |  |  |  |  | <ul><li>395ff55f-eac9-40d1-ab6d-a9a8f5d69477<ul><li>Maximum Value Operator; Maximum Domain Operator: Blank Concept</li><li>Minimum Value Operator; Minimum Domain Operator: Blank Concept</li><li>Allowable Range Maximum Value: 0.0</li><li>Allowable Range Minimum Value: 0.0</li><li>Example UCUM Units: </li></ul></li></ul> |

";

        private static readonly string DEVICE_08426950447627 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| HemosIL RecombiPlasTin 2G (20 mL) | 08426950447627 | 80ccf451-1d71-5f2c-9c5b-6db37909fc45 | 82f17ba1-2b9b-4320-8c39-15e5b2d9cf8a | <ul><li>5902-2</li></ul> | <ul><li>08426950784081</li><li>08426950729242</li><li>08426950784067</li></ul> | <ul><li>Plasma specimen with citrate (specimen)</li></ul> | Seconds |  | <ul><li>6277fb69-ca18-4923-8a2b-b19d6ed04489<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 12.5</li><li>Allowable Range Minimum Value: 9.4</li><li>Example UCUM Units: seconds</li></ul></li></ul> |
| HemosIL RecombiPlasTin 2G (20 mL) | 08426950447627 | 80ccf451-1d71-5f2c-9c5b-6db37909fc45 | 19ab66f2-964f-4709-bce5-cd2cb414a84c | <ul><li>3255-7</li></ul> | <ul><li>08426950784081</li><li>08426950729242</li><li>08426950784067</li></ul> | <ul><li>Plasma specimen with citrate (specimen)</li></ul> | mg/dL |  | <ul><li>8c5f88f5-253a-4587-b69c-b839a3ce6c47<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 471.0</li><li>Allowable Range Minimum Value: 276.0</li><li>Example UCUM Units: mg/dL</li></ul></li></ul> |

";

        private static readonly string DEVICE_08056771101905 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| LIAISON® CMV IgG | 08056771101905 | 3a372249-4dfc-549b-8e90-a064b7a26beb | 25a9e0e3-d73c-4f6f-912c-151929b29a54 | <ul><li>13949-3</li></ul> | <ul><li>08056771101455</li><li>08056771102148</li></ul> | <ul><li>Serum specimen (specimen)</li></ul> | U/mL | <ul><li>d9b5666a-e0e3-446a-9627-7d3ddfe49d52<ul><li>Reference Range Population: Equivocal</li><li>Maximum Value Operator; Maximum Domain Operator: Less than</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 0.7</li><li>Minimum Value; Min Value: 0.6</li><li>Example UCUM Units: U/mL</li></ul></li><li>a7b32599-edab-4a66-a8a4-a0fec1d93e26<ul><li>Reference Range Population: Negative</li><li>Maximum Value Operator; Maximum Domain Operator: Less than</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 0.6</li><li>Minimum Value; Min Value: 0.0</li><li>Example UCUM Units: U/mL</li></ul></li><li>f2a447a8-9167-409a-bede-e991ff2a9e4a<ul><li>Reference Range Population: Positive</li><li>Maximum Value Operator; Maximum Domain Operator: Greater than</li><li>Minimum Value Operator; Minimum Domain Operator: Equal to</li><li>Maximum Value; Max Value: 0.7</li><li>Minimum Value; Min Value: 0.6</li><li>Example UCUM Units: U/mL</li></ul></li></ul> |  |

";

        private static readonly string DEVICE_04560189282919 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Tosoh Automated Glycohemoglobin Analyzer HLC-723G8 HLC-723G8 | 04560189282919 | fdb008db-6b4c-583b-b2f6-fcc221a583bc | 89391237-1c6f-48fb-8565-0b9b701a23f5 | <ul><li>17856-6</li></ul> | <ul><li>04560189282919</li></ul> | <ul><li>Whole blood specimen (specimen)</li></ul> | % |  | <ul><li>d2d4fcaf-7f83-4133-ae2c-14ce042cfbe1<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 6.0</li><li>Allowable Range Minimum Value: 4.0</li><li>Example UCUM Units: %</li></ul></li></ul> |

";

        private static readonly string DEVICE_20758750008343 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| VITROS Chemistry Products ALKP Slides | 20758750008343 | bfaab2dc-a41b-50a5-8ed1-f5a2f79703aa | 48ece425-e8ab-444e-ac81-15e56b4b1c0e | <ul><li>6768-6</li></ul> | <ul><li>10758750031610</li><li>10758750012343</li><li>10758750002740</li><li>10758750001330</li><li>10758750002054</li><li>10758750031986</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | U/L, ukat/L |  | <ul><li>451b47b3-6c1e-407c-be45-f68d4f7d0716<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 126.0</li><li>Allowable Range Minimum Value: 38.0</li><li>Example UCUM Units: U/L</li></ul></li><li>190d92e7-0aac-4c55-81eb-402e2c7236c6<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 2.1</li><li>Allowable Range Minimum Value: 0.6</li><li>Example UCUM Units: ukat/L</li></ul></li></ul> |

";

        private static readonly string DEVICE_10758750002849 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| VITROS Chemistry Products CREA Slides | 10758750002849 | 41ae2887-19b2-52d0-ba01-903c5bbe3079 | 90a69aeb-70df-4210-978f-59f08c81bb82 | <ul><li>2160-0</li></ul> | <ul><li>10758750031610</li><li>10758750002740</li><li>10758750012343</li><li>10758750001330</li><li>10758750002054</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | mg/dL | <ul><li>38841b66-205e-429a-b5e3-54f9e5775ecb<ul><li>Reference Range Population: Female</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 1.04</li><li>Minimum Value; Min Value: 0.52</li><li>Example UCUM Units: mg/dL</li></ul></li><li>3905ceca-60fa-4513-b9ae-0210362f84ab<ul><li>Reference Range Population: Female</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 1.04</li><li>Minimum Value; Min Value: 0.52</li><li>Example UCUM Units: mg/mL</li></ul></li></ul> |  |
| VITROS Chemistry Products CREA Slides | 10758750002849 | 41ae2887-19b2-52d0-ba01-903c5bbe3079 | f5d6e53e-90d8-4a33-9574-3ffd4a502b99 | <ul><li>20624-3</li></ul> | <ul><li>10758750031610</li><li>10758750002740</li><li>10758750012343</li><li>10758750001330</li><li>10758750002054</li></ul> | <ul><li>Urine specimen (specimen)</li></ul> | mg/day | <ul><li>90a6c814-4ebf-4ece-8e2c-4f8d47f118ad<ul><li>Reference Range Population: Male</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 2000.0</li><li>Minimum Value; Min Value: 1000.0</li><li>Example UCUM Units: mg/day</li></ul></li><li>2dd90c83-9611-4b50-ada4-b6ae1ec678bc<ul><li>Reference Range Population: Female</li><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Maximum Value; Max Value: 1800.0</li><li>Minimum Value; Min Value: 800.0</li><li>Example UCUM Units: mg/day</li></ul></li></ul> |  |

";

        private static readonly string DEVICE_20758750002501 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| VITROS Immunodiagnostic Products Troponin I ES Reagent Pack | 20758750002501 | 183bb08a-dca7-5150-b9bd-c368b51b4c09 | c2c18658-f448-470d-aa89-8f10e92fbb7f | <ul><li>10839-9</li></ul> | <ul><li>10758750031610</li><li>10758750000272</li><li>10758750002979</li><li>10758750002740</li></ul> | <ul><li>Plasma specimen (specimen)</li><li>Serum specimen (specimen)</li></ul> | ng/mL |  |  |

";

        private static readonly string DEVICE_10758750004522 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| VITROS XT Chemistry Products ALB Slides | 10758750004522 | 255339d8-0deb-57da-954d-21b80eea0b67 | 4191fbea-3a14-4571-97cb-78bcadceb878 | <ul><li>61151-7</li></ul> | <ul><li>10758750031610</li><li>10758750035656</li><li>10758750001330</li><li>10758750002054</li><li>10758750012343</li><li>10758750002740</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | g/dL (SI Units: g/L; Alternate units umol/L) |  | <ul><li>1d1a1fd5-e897-4af0-8aaf-ff247599aab3<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 6.0</li><li>Allowable Range Minimum Value: 1.0</li><li>Example UCUM Units: g/dL</li></ul></li><li>6c6325ea-a101-418f-bd81-a5729be9c699<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 60.0</li><li>Allowable Range Minimum Value: 10.0</li><li>Example UCUM Units: g/L</li></ul></li><li>ace9c3a7-680b-4209-bd36-e3c623913fa5<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 912.0</li><li>Allowable Range Minimum Value: 152.0</li><li>Example UCUM Units: umol/L</li></ul></li></ul> |

";

        private static readonly string DEVICE_07332940008604 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Xpert Xpress CoV-2/Flu/RSV plus | 07332940008604 | 1bdab7cf-00d4-5ff8-9108-66644b524b59 | c18fb307-fb4f-4fdc-988d-2db6dfa05ba4 | <ul><li>95941-1</li><li>94500-6</li><li>95422-2</li></ul> | <ul><li>07332940004071</li><li>07332940003364</li><li>07332940006761</li><li>07332940006754</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | IU/mL |  |  |
| Xpert Xpress CoV-2/Flu/RSV plus | 07332940008604 | 1bdab7cf-00d4-5ff8-9108-66644b524b59 | f30a5d0b-161b-456c-88a7-3079349774d5 | <ul><li>95941-1</li><li>85477-8</li><li>92142-9</li><li>95422-2</li><li>85476-0</li><li>92882-0</li></ul> | <ul><li>07332940004071</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | FFU/mL |  |  |
| Xpert Xpress CoV-2/Flu/RSV plus | 07332940008604 | 1bdab7cf-00d4-5ff8-9108-66644b524b59 | 29d5ab1e-2030-40be-8573-601f8216e48c | <ul><li>92141-1</li><li>92882-0</li><li>85476-0</li><li>95422-2</li><li>85478-6</li><li>95941-1</li></ul> | <ul><li>07332940004071</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | CEID50/mL |  |  |
| Xpert Xpress CoV-2/Flu/RSV plus | 07332940008604 | 1bdab7cf-00d4-5ff8-9108-66644b524b59 | a0a12f8a-659d-4a62-9bf2-56bf6b53c40c | <ul><li>92131-2</li><li>95941-1</li><li>85479-4</li><li>95422-2</li><li>85476-0</li><li>92882-0</li></ul> | <ul><li>07332940004071</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | TCID50/mL |  |  |

";

        private static readonly string DEVICE_07332940008581 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Xpert® Xpress CoV-2 plus | 07332940008581 | 3ad4d4ed-df82-504c-9ceb-ec99920f145c | 1eb10be4-d909-4c92-b347-9ca31cc6d3ed | <ul><li>94500-6</li></ul> | <ul><li>07332940004071</li></ul> | <ul><li>Nasopharyngeal swab (specimen)</li><li>Swab of internal nose (specimen)</li></ul> | Copies/mL |  |  |

";

        private static readonly string DEVICE_08426950485605_2 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| alkaline phosphatase | 08426950485605 | 3be68d35-c40a-5708-ba68-a38a9f6f2c63 | 48ece425-e8ab-444e-ac81-15e56b4b1c0e | <ul><li>6768-6</li></ul> | <ul><li>10758750031610</li><li>10758750012343</li><li>10758750002740</li><li>10758750001330</li><li>10758750002054</li><li>10758750031986</li></ul> | <ul><li>Serum specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | U/L, ukat/L |  | <ul><li>451b47b3-6c1e-407c-be45-f68d4f7d0716<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 126.0</li><li>Allowable Range Minimum Value: 38.0</li><li>Example UCUM Units: U/L</li></ul></li><li>190d92e7-0aac-4c55-81eb-402e2c7236c6<ul><li>Maximum Value Operator; Maximum Domain Operator: Less than or equal to</li><li>Minimum Value Operator; Minimum Domain Operator: Greater than or equal to</li><li>Allowable Range Maximum Value: 2.1</li><li>Allowable Range Minimum Value: 0.6</li><li>Example UCUM Units: ukat/L</li></ul></li></ul> |

";

        private static readonly string DEVICE_10054749000187 = 
@"| Device Name | Device Identifier | Device Concept UUID | Test Performed Semantic UUID | LOINC Code Identifier(s) | Instrument Concept Global Standards One Identifier(s) | Specimen Concept FQN(s) | Units of Measure | Population Reference Range Semantic UUID(s) | Quantitative Allowed Results Range Pattern UUID(s) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| i-STAT 03P90-25 | 10054749000187 | dbebb268-e65b-5314-bde7-d75ab95d5d5a | c5a66690-3917-4133-bc6e-f64d7cba6e85 | <ul><li>10839-9</li></ul> | <ul><li>00054749000340</li><li>00054749003709</li></ul> | <ul><li>Whole blood specimen (specimen)</li><li>Plasma specimen (specimen)</li></ul> | ng/mL |  |  |

";
    }
}
