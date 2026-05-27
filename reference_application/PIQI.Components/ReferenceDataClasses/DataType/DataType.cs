namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the root container for a library of data types.
    /// Used to organize all available data types in the system.
    /// </summary>
    public class DataTypeRoot
    {
        /// <summary>
        /// The collection of data types in this library.
        /// </summary>
        public List<DataType> DataTypeLibrary { get; set; } = new List<DataType>();
    }

    /// <summary>
    /// Represents a data type that can be applied to a message attribute or value.
    /// </summary>
    public class DataType
    {
        /// <summary>
        /// The short code representing the data type (e.g., "ST" for string).
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// The human-readable name of the data type.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Indicates if this data type is coded (e.g., CodeableConcept).
        /// </summary>
        public bool IsCoded { get; set; }

        /// <summary>
        /// Indicates if this data type represents a numeric value.
        /// </summary>
        public bool IsNumeric { get; set; }

        /// <summary>
        /// Indicates if this data type represents a range of values.
        /// </summary>
        public bool IsRange { get; set; }
    }
}
