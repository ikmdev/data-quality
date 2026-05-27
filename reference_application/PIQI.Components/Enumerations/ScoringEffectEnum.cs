namespace PIQI.Components.Models
{
    /// <summary>
    /// Defines the scoring effect of an evaluation criterion.
    /// </summary>
    public enum ScoringEffectEnum
    {
        /// <summary>
        /// The criterion affects the score of the evaluation.
        /// </summary>
        Scoring = 1,

        /// <summary>
        /// The criterion is informational and does not affect the score.
        /// </summary>
        Informational = 2
    }
}
