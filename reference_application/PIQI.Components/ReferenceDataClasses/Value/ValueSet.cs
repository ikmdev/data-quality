namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the root object containing a library of value sets.
    /// </summary>
    public class ValueSetListRoot
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="ValueSet"/> objects in the library.
        /// </summary>
        public List<ValueSet> ValueSetLibrary { get; set; } = new();
    }

    /// <summary>
    /// Represents a FHIR value set, including its mnemonic, name, description, URI, and associated codings.
    /// </summary>
    public class ValueSet
    {
        #region Properties

        /// <summary>
        /// Gets or sets the descriptive name of the value set.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic identifier for the value set.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// Gets or sets a description of the value set.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the FHIR URI associated with this value set.
        /// </summary>
        public string? FhirUri { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Coding"/> objects associated with this value set.
        /// </summary>
        public List<Coding> CodingList { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet"/> class with an empty coding list.
        /// </summary>
        public ValueSet()
        {
            CodingList = new List<Coding>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueSet"/> class with the specified mnemonic.
        /// </summary>
        /// <param name="mnemonic">The mnemonic identifier for the value set.</param>
        public ValueSet(string mnemonic)
        {
            Mnemonic = mnemonic;
            CodingList = new List<Coding>();
        }

        #endregion
    }
}
