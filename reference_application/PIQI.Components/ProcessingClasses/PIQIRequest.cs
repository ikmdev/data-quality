using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Request object sent to the PIQI engine for evaluation and scoring.
    /// </summary>
    public class PIQIRequest
    {
        /// <summary>
        /// Identifier for the data provider submitting the request.
        /// Optional if included in the message.
        /// </summary>
        public string? DataProviderID { get; set; }

        /// <summary>
        /// Identifier for the specific data source within the provider.
        /// Optional if included in the message.
        /// </summary>
        public string? DataSourceID { get; set; }

        /// <summary>
        /// Unique identifier for the message being evaluated.
        /// Optional if included in the message.
        /// </summary>
        public string? MessageID { get; set; }

        /// <summary>
        /// Mnemonic code representing the PIQI model to use for evaluation.
        /// </summary>
        [Required(ErrorMessage = "PIQIModelMnemonic is required")]
        public string PIQIModelMnemonic { get; set; }

        /// <summary>
        /// Mnemonic code representing the evaluation rubric to apply.
        /// </summary>
        [Required(ErrorMessage = "EvaluationRubricMnemonic is required")]
        public string EvaluationRubricMnemonic { get; set; }

        /// <summary>
        /// Timestamp when the request was created. Defaults to the current time.
        /// </summary>
        [JsonIgnore]
        public DateTime CreationDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Raw message data that will be evaluated by the PIQI engine.
        /// </summary>
        [Required(ErrorMessage = "MessageData is required")]
        public string MessageData { get; set; }
    }
}
