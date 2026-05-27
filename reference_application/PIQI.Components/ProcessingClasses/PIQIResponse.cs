using System.Diagnostics;

namespace PIQI.Components.Models
{
    /// <summary>
    /// Response object for API calls to the PIQI engine.
    /// Contains information about the success of the call, scoring data, and optionally an audited message.
    /// </summary>
    public class PIQIResponse
    {
        #region Properties

        /// <summary>
        /// Indicates whether the API call was successful.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// Contains the error message if processing failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Time it took to process the message in milliseconds
        /// </summary>
        public long? ElapsedTimeInMS { get; set; }

        /// <summary>
        /// Holds the scoring data returned from the PIQI engine.
        /// </summary>
        public PIQIStatResponse ScoringData { get; set; }

        /// <summary>
        /// Optional audited message related to the API call.
        /// </summary>
        public string? AuditedMessage { get; set; }

        #endregion

        #region Success/Failure Methods

        /// <summary>
        /// Marks the result as successful without scoring data.
        /// </summary>
        public void Succeed()
        {
            Succeeded = true;
        }

        /// <summary>
        /// Marks the result as successful and sets the scoring data.
        /// </summary>
        /// <param name="scoringData">The scoring data returned from the PIQI engine.</param>
        /// <param name="stopwatch">The processing time returned from the PIQI engine.</param>
        public void Succeed(PIQIStatResponse scoringData, Stopwatch stopwatch)
        {
            Succeeded = true;
            ScoringData = scoringData;
            ElapsedTimeInMS = stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Marks the result as failed with a custom error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        public void Fail(string errorMessage)
        {
            Succeeded = false;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Marks the result as failed and extracts the error message from an exception.
        /// </summary>
        /// <param name="ex">The exception that caused the failure.</param>
        public void Fail(Exception ex)
        {
            Succeeded = false;
            ErrorMessage = ex.Message;
        }

        #endregion
    }
}
