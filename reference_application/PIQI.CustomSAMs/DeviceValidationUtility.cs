using System;
using System.Collections.Generic;
using System.Linq;

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
        /// The master mapping table containing valid attributes for various devices.
        /// Format: Markdown-style table with headers and data rows.
        /// </summary>
        private static readonly string DEVICE_MAPPING_TABLE = 
@"| Device Name         | Device Identifier   | Device Concept UUID                  | Test Performed Semantic UUID         | LOINC Code Identifier(s)                  | Instrument Concept Global Standards One Identifier(s)   | Specimen Concept FQN(s)                                                       | Units of Measure   | Population Reference Range Semantic UUID(s)   | Quantitative Allowed Results Range Pattern UUID(s)                                                                                                                                                                                                                                                                                                         |
|---------------------|---------------------|--------------------------------------|--------------------------------------|-------------------------------------------|---------------------------------------------------------|-------------------------------------------------------------------------------|--------------------|-----------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Device Name 1       | 1 | Device Concept UUID 1                | Test Performed Semantic UUID 1       | LOINC Code Identifier(s) 1                | Instrument Concept Global Standards One Identifier(s) 1 | Specimen Concept FQN(s) 1                                                     | Units of Measure 1 | Population Reference Range Semantic UUID(s) 1 | Quantitative Allowed Results Range Pattern UUID(s) 1                                                                                                                                                                                                                                                                                                       |
| Device Name 1       | 1 | Device Concept UUID 1                | Test Performed Semantic UUID 2       | LOINC Code Identifier(s) 2                | Instrument Concept Global Standards One Identifier(s) 2 | Specimen Concept FQN(s) 2                                                     | Units of Measure 2 | Population Reference Range Semantic UUID(s) 2 | Quantitative Allowed Results Range Pattern UUID(s) 2                                                                                                                                                                                                                                                                                                       |
| Device Name 1       | 1 | Device Concept UUID 1                | Test Performed Semantic UUID 3       | 2162-6                | Instrument Concept Global Standards One Identifier(s) 3 | Specimen Concept FQN(s) 3                                                     | Units of Measure 3 | Population Reference Range Semantic UUID(s) 3 | Quantitative Allowed Results Range Pattern UUID(s) 3                                                                                                                                                                                                                                                                                                       |";

        /// <summary>
        /// Retrieves the mapping table string for a specific device.
        /// </summary>
        /// <param name="deviceId">The identifier of the device.</param>
        /// <returns>The mapping table string if found; otherwise, null.</returns>
        public static string GetTableForDevice(string deviceId)
        {
            return deviceId switch { "1" => DEVICE_MAPPING_TABLE, _ => null };
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
                if (cols.Length > columnIndex && predicate(cols[columnIndex])) return true;
            }
            return false;
        }
    }
}
