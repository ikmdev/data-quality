namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the root object for a collection of code systems.
    /// Typically used to wrap code systems in JSON.
    /// </summary>
    public class CodeSystemRoot
    {
        /// <summary>
        /// The list of code systems contained in the library.
        /// </summary>
        public List<CodeSystem> CodeSystemLibrary { get; set; } = new List<CodeSystem>();
    }

    /// <summary>
    /// Represents a terminology or coding system with identifying information.
    /// </summary>
    public class CodeSystem
    {
        /// <summary>
        /// The name of the code system.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The mnemonic or short identifier for the code system.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// Optional FHIR URI for the code system.
        /// </summary>
        public string? FhirUri { get; set; }

        /// <summary>
        /// A list of identifiers associated with this code system.
        /// </summary>
        public List<string> CodeSystemIdentifiers { get; set; } = new List<string>();
    }
}
