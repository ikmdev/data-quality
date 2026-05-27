using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Root container for a collection of SAM (Scoring and Analysis Module) objects.
    /// </summary>
    public class SAMRoot
    {
        /// <summary>
        /// List of SAM objects in the library.
        /// </summary>
        public List<SAM> SAMLibrary { get; set; } = null!;
    }

    /// <summary>
    /// Represents a Scoring and Analysis Module (SAM) with metadata, parameters, and source information.
    /// </summary>
    public class SAM
    {
        /// <summary>
        /// The name of the SAM.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The optional unique identifier of the SAM
        /// </summary>
        public Guid? SamUID { get; set; }

        /// <summary>
        /// Unique mnemonic identifier for the SAM.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// Optional description of the SAM.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The date and time when the SAM was created.
        /// </summary>
        public string? CreatedDateTime { get; set; }

        /// <summary>
        /// The date and time when the SAM was last modified.
        /// </summary>
        public string? ModifiedDateTime { get; set; }

        /// <summary>
        /// Optional mnemonic indicating the execution type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SAMExecutionTypeEnum? ExecutionType { get; set; }

        /// <summary>
        /// Optional reference information for execution.
        /// </summary>
        public string? ExecutionReference { get; set; }

        /// <summary>
        /// Optional alias representing a failure outcome.
        /// </summary>
        public string? FailName { get; set; }

        /// <summary>
        /// Optional input type for the SAM.
        /// </summary>
        public string? InputType { get; set; }

        /// <summary>
        /// Optional mnemonic of a prerequisite SAM that must execute first.
        /// </summary>
        public string? PrerequisiteSAMMnemonic { get; set; }

        /// <summary>
        /// Optional HDQT dimension mnemonic associated with this SAM.
        /// </summary>
        public string? HDQTDimensionMnemonic { get; set; }

        /// <summary>
        /// The PIQI model associated with this SAM.
        /// </summary>
        public PIQIModel PIQIModel { get; set; } = null!;

        /// <summary>
        /// Source associated with the SAM.
        /// </summary>
        public PIQISource Source { get; set; } = null!;

        /// <summary>
        /// List of parameters used by the SAM.
        /// </summary>
        public List<SAMParameter>? Parameters { get; set; }
    }
}
