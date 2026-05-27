namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents an evaluation rubric which contains a set of criteria for scoring entities.
    /// </summary>
    public class EvaluationRubric
    {
        /// <summary>
        /// The name of the rubric.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Optional description providing details about the rubric.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The unique mnemonic of the rubric.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// The version identifier of the rubric.
        /// </summary>
        public string Version { get; set; } = null!;

        /// <summary>
        /// The date and time the rubric was last modified.
        /// </summary>
        public string ModifiedDateTime { get; set; } = null!;

        /// <summary>
        /// The date and time the rubric was created.
        /// </summary>
        public string CreationDateTime { get; set; } = null!;

        /// <summary>
        /// The source system or organization for the rubric.
        /// </summary>
        public PIQISource Source { get; set; } = null!;

        /// <summary>
        /// The associated PIQI model for the rubric.
        /// </summary>
        public PIQIModel Model { get; set; } = null!;

        /// <summary>
        /// The list of evaluation criteria contained in the rubric.
        /// </summary>
        public List<EvaluationCriterion> Criteria { get; set; } = new List<EvaluationCriterion>();
    }
}
