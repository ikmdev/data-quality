namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents the result of evaluating a specific criterion against an evaluation item using a SAM.
    /// Contains status, conditional/dependency flags, and metadata about the evaluation.
    /// </summary>
    public class EvaluationResult
    {
        #region Properties

        /// <summary>
        /// Gets or sets a unique identifier for this evaluation result.
        /// </summary>
        public Guid ResultUID { get; set; }

        /// <summary>
        /// Gets or sets the key identifying this evaluation result.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the evaluation item to which this result belongs.
        /// </summary>
        public EvaluationItem EvaluationItem { get; set; }

        /// <summary>
        /// Gets or sets the evaluation criterion that caused this evaluation result.
        /// </summary>
        public EvaluationCriterion Criterion { get; set; }

        /// <summary>
        /// Gets or sets the SAM object that performed the evaluation.
        /// </summary>
        public SAM Sam { get; set; }

        /// <summary>
        /// Gets or sets the type of entity this result belongs to (Root, Class, Element, or Attribute).
        /// </summary>
        public EntityItemTypeEnum? ItemType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this evaluation result is conditional on other evaluations.
        /// </summary>
        public bool IsConditional { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this evaluation result is dependent on other evaluation results.
        /// </summary>
        public bool IsDependent { get; set; }

        /// <summary>
        /// Gets a value indicating whether this evaluation result contributes to scoring.
        /// </summary>
        public bool IsScoring => Criterion.ScoringEffect == ScoringEffectEnum.Scoring;

        /// <summary>
        /// Gets a value indicating whether this evaluation result is considered critical.
        /// </summary>
        public bool IsCritical => Criterion.CriticalityIndicator;

        /// <summary>
        /// Gets or sets the condition result that this evaluation may depend on.
        /// </summary>
        public EvaluationResult ConditionResult { get; set; }

        /// <summary>
        /// Gets or sets the dependent result associated with this evaluation.
        /// </summary>
        public EvaluationResult DependentResult { get; set; }

        /// <summary>
        /// Gets or sets the overall evaluation state of this result (Pending, Passed, Failed, Skipped).
        /// </summary>
        public ProcessStateEnum EvalResult { get; set; }

        /// <summary>
        /// Gets or sets a custom error message, if additional information is required.
        /// </summary>
        public string CustomErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this evaluation was actually performed.
        /// May be false if the result inherits a condition or fail status.
        /// </summary>
        public bool EvalPerformed { get; set; }

        /// <summary>
        /// Gets or sets the SAM that caused this evaluation to be skipped, if applicable.
        /// </summary>
        public SAM SkipSam { get; set; }

        /// <summary>
        /// Gets or sets the SAM that caused this evaluation to fail, if applicable.
        /// </summary>
        public SAM FailSam { get; set; }

        /// <summary>
        /// Gets a value indicating whether the evaluation is still pending.
        /// </summary>
        public bool EvalPending => EvalResult == ProcessStateEnum.Pending;

        /// <summary>
        /// Gets a value indicating whether the evaluation passed.
        /// </summary>
        public bool EvalPassed => EvalResult == ProcessStateEnum.Passed;

        /// <summary>
        /// Gets a value indicating whether the evaluation was skipped.
        /// </summary>
        public bool EvalSkipped => EvalResult == ProcessStateEnum.Skipped;

        /// <summary>
        /// Gets a value indicating whether the evaluation failed.
        /// </summary>
        public bool EvalFailed => EvalResult == ProcessStateEnum.Failed;

        /// <summary>
        /// Gets the mnemonic of the entity associated with this evaluation result.
        /// </summary>
        public string EntityMnemonic => EvaluationItem.Entity.Mnemonic;

        /// <summary>
        /// Gets the mnemonic of the data class associated with this evaluation result, if applicable.
        /// </summary>
        public string? DataClassMnemonic => EvaluationItem.ClassEntityMnemonic;

        /// <summary>
        /// Gets the mnemonic of the SAM that performed this evaluation.
        /// </summary>
        public string SamMnemonic => Sam.Mnemonic;

        /// <summary>
        /// Gets the mnemonic of the SAM that caused the evaluation to be skipped, if any.
        /// </summary>
        public string? SkipSamMnemonic => SkipSam?.Mnemonic;

        /// <summary>
        /// Gets the mnemonic of the SAM that caused the evaluation to fail, if any.
        /// </summary>
        public string? FailSamMnemonic => FailSam?.Mnemonic;

        /// <summary>
        /// Gets the name of the entity associated with this evaluation result.
        /// </summary>
        public string EntityName => EvaluationItem.Entity.Name ?? EvaluationItem.Entity.FieldName;

        /// <summary>
        /// Gets the display name of the SAM, considering any criterion-specific override.
        /// </summary>
        public string SamDisplayName => !string.IsNullOrEmpty(Criterion.SamNameOverride) ? Criterion.SamNameOverride : Sam.Name;

        /// <summary>
        /// Gets the scoring weight defined by the evaluation criterion.
        /// </summary>
        public int ScoringWeight => Criterion.ScoringWeight;

        /// <summary>
        /// Optional reason for a skipped or failed evaluation.
        /// </summary>
        public string? Reason { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationResult"/> class for a specific evaluation item,
        /// criterion, and SAM, and sets the conditional and dependent flags.
        /// </summary>
        /// <param name="evaluationItem">The <see cref="EvaluationItem"/> that this result belongs to.</param>
        /// <param name="criterion">The <see cref="EvaluationCriterion"/> that triggered this evaluation.</param>
        /// <param name="sam">The <see cref="SAM"/> object that performed the evaluation.</param>
        /// <param name="isConditional">
        /// Indicates whether this result is part of a conditional evaluation within the root evaluation.
        /// </param>
        /// <param name="isDependent">
        /// Indicates whether this result is part of a dependent evaluation tree within the root evaluation.
        /// </param>
        public EvaluationResult(EvaluationItem evaluationItem, EvaluationCriterion criterion, SAM sam, bool isConditional, bool isDependent)
        {
            ResultUID = Guid.NewGuid();

            Key = evaluationItem.Key;
            EvaluationItem = evaluationItem;
            Criterion = criterion;
            Sam = sam;
            ItemType = evaluationItem.ItemType;

            IsConditional = isConditional;
            IsDependent = isDependent;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Marks this evaluation result as passed and sets the <see cref="EvalPerformed"/> flag to true.
        /// </summary>
        public void Pass()
        {
            EvalResult = ProcessStateEnum.Passed;
            EvalPerformed = true;
        }

        /// <summary>
        /// Marks this evaluation result as skipped and sets the <see cref="EvalPerformed"/> flag appropriately.
        /// If <paramref name="skipSam"/> is null, the current SAM is used; otherwise, the provided SAM is recorded as the cause.
        /// </summary>
        /// <param name="skipSam">The SAM that caused the evaluation to be skipped. Can be null.</param>
        public void Skip(SAM skipSam, string? skipReason = null)
        {
            EvalResult = ProcessStateEnum.Skipped;
            EvalPerformed = (skipSam == null);
            SkipSam = (skipSam == null ? Sam : skipSam);
            Reason = skipReason;
        }

        /// <summary>
        /// Marks this evaluation result as failed and sets the <see cref="EvalPerformed"/> flag appropriately.
        /// If <paramref name="failSam"/> is null, the current SAM is used; otherwise, the provided SAM is recorded as the cause.
        /// </summary>
        /// <param name="failSam">The SAM that caused the evaluation to fail. Can be null.</param>
        /// <param name="failReason">The reason that caused the evaluation to fail. Can be null.</param>
        public void Fail(SAM failSam, string? failReason = null)
        {
            EvalResult = ProcessStateEnum.Failed;
            EvalPerformed = (failSam == null);
            FailSam = (failSam == null ? Sam : failSam);
            Reason = failReason;
        }

        #endregion

    }
}
