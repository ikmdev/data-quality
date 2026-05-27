namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a criterion used to evaluate an entity, including scoring, parameters, and conditional logic.
    /// </summary>
    public class EvaluationCriterion
    {
        /// <summary>
        /// The sequence number of the criterion within an evaluation.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// A textual description of the evaluation criterion.
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// The data class associated with this criterion.
        /// </summary>
        public string DataClass { get; set; } = null!;

        /// <summary>
        /// The effect of the criterion on scoring.
        /// </summary>
        public ScoringEffectEnum ScoringEffect { get; set; }

        /// <summary>
        /// The weight assigned to this criterion in the scoring calculation.
        /// </summary>
        public int ScoringWeight { get; set; }

        /// <summary>
        /// Indicates whether this criterion is critical.
        /// </summary>
        public bool CriticalityIndicator { get; set; }

        /// <summary>
        /// The entity that this evaluation criterion applies to.
        /// </summary>
        public string Entity { get; set; } = null!;

        /// <summary>
        /// The SAM mnemonic associated with this criterion.
        /// </summary>
        public string SAMMnemonic { get; set; } = null!;

        /// <summary>
        /// If the criterion is a RESTful API, this is the associated URL for processing.
        /// </summary>
        public string? ProcessingURL { get; set; } = null!;

        /// <summary>
        /// Optional override for the name of the criterion, ignores status.
        /// </summary>
        public string? SamNameOverride { get; set; }

        /// <summary>
        /// Optional override for the success name of the criterion.
        /// </summary>
        public string? SuccessNameOverride { get; set; }

        /// <summary>
        /// Optional override for the failure name of the criterion.
        /// </summary>
        public string? FailureNameOverride { get; set; }

        /// <summary>
        /// Conditional SAM logic, if any.
        /// </summary>
        public string ConditionalSAM { get; set; } = null!;

        /// <summary>
        /// The parameters associated with the SAM logic for this criterion.
        /// </summary>
        public List<EvaluationCriteriaParameter> SAMParameters { get; set; } = new List<EvaluationCriteriaParameter>();

        /// <summary>
        /// The parameters associated with the conditional SAM logic for this criterion.
        /// </summary>
        public List<EvaluationCriteriaParameter> ConditionalSAMParameters { get; set; } = new List<EvaluationCriteriaParameter>();
    }
}
