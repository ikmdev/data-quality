using Newtonsoft.Json;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the root container for evaluation profiles.
    /// </summary>
    public class ReferenceDataProfileRoot
    {
        /// <summary>
        /// A collection of evaluation profiles.
        /// </summary>
        [JsonProperty("EvaluationProfileLibrary")]
        public List<ReferenceDataProfile>? EvaluationProfiles { get; set; }

        /// <summary>
        /// A collection of model profiles.
        /// </summary>
        [JsonProperty("ModelLibrary")]
        public List<ReferenceDataProfile>? ModelProfiles { get; set; }
    }

    /// <summary>
    /// Represents an individual evaluation profile with basic metadata.
    /// </summary>
    public class ReferenceDataProfile
    {
        #region Properties

        /// <summary>
        /// The name of the evaluation profile.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The unique mnemonic of the evaluation profile.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// The file path where the evaluation profile is stored.
        /// </summary>
        public string FilePath { get; set; } = null!;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataProfile"/> class.
        /// </summary>
        /// <param name="name">The display name of the profile.</param>
        /// <param name="mnemonic">The unique mnemonic identifier associated with the profile.</param>
        /// <param name="filePath">
        /// An optional file path for the profile. 
        /// If not provided, the profile will not be associated with a file.
        /// </param>
        public ReferenceDataProfile(string name, string mnemonic, string? filePath = null)
        {
            Name = name;
            Mnemonic = mnemonic;
            if (filePath != null)
                FilePath = filePath;
        }

        #endregion
    }
}
