namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a request to evaluate a PIQI Scoring and Assessment Method (SAM).
    /// Encapsulates the input message data and any additional parameters required for processing.
    /// </summary>
    public class PIQISAMRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the message object containing the data to be processed by the SAM.
        /// </summary>
        public object EvaluationObject { get; set; }                   // The message data to process

        /// <summary>
        /// Gets or sets the list of additional parameters provided to the SAM.
        /// </summary>
        /// <remarks>
        /// Each parameter is represented as a <see cref="Tuple{T1, T2}"/>, where:
        /// <list type="bullet">
        ///   <item><description><c>Item1</c> is the parameter name.</description></item>
        ///   <item><description><c>Item2</c> is the parameter value.</description></item>
        /// </list>
        /// </remarks>
        public List<Tuple<string, string>> ParmList { get; set; }   // Any additional parameters as a list of name/value pairs

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new parameter to the request.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <remarks>
        /// If <see cref="ParmList"/> is <c>null</c>, it will be initialized before the parameter is added.
        /// </remarks>
        public void AddParameter(string name, string value)
        {
            if (ParmList == null) ParmList = new List<Tuple<string, string>>();
            ParmList.Add(new Tuple<string, string>(name, value));
        }

        /// <summary>
        /// Removes parameters from the request by name.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <remarks>
        /// If <see cref="ParmList"/> is <c>null</c>, no action will be taken.
        /// All parameters matching the given name will be removed.
        /// </remarks>
        public void RemoveParameter(string name)
        {
            if (ParmList == null) return;
            ParmList.RemoveAll(p => string.Equals(p.Item1, name, StringComparison.OrdinalIgnoreCase));
        }


        #endregion
    }
}
