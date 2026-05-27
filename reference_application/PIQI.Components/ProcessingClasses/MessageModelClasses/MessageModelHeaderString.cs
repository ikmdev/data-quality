namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a string value in the message header, supporting original, default, and override values.
    /// </summary>
    public class MessageModelHeaderString
    {
        #region Properties

        /// <summary>
        /// The original value of the string as provided in the message.
        /// </summary>
        public string OriginalValue { get; set; }

        /// <summary>
        /// The default value of the string, used if no original or override value is present.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// The override value of the string, which takes precedence over the original and default values.
        /// </summary>
        public string OverrideValue { get; set; }

        /// <summary>
        /// Gets the effective string value, considering override, original, and default values in that order.
        /// </summary>
        public string Value
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OverrideValue)) return OverrideValue;
                else if (!string.IsNullOrWhiteSpace(OriginalValue)) return OriginalValue;
                else if (!string.IsNullOrWhiteSpace(DefaultValue)) return DefaultValue;
                else return null;
            }
        }

        /// <summary>
        /// Gets the state of the string value, corresponding to Overridden, Original, Default, or None.
        /// </summary>
        public int ValueState
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OverrideValue)) return (int)MessageModelHeaderValueStateEnum.Overridden;
                else if (!string.IsNullOrWhiteSpace(OriginalValue)) return (int)MessageModelHeaderValueStateEnum.Original;
                else if (!string.IsNullOrWhiteSpace(DefaultValue)) return (int)MessageModelHeaderValueStateEnum.Default;
                else return (int)MessageModelHeaderValueStateEnum.None;
            }
        }

        /// <summary>
        /// Indicates whether a value is set (either override, original, or default).
        /// </summary>
        public bool HasValue => (ValueState != (int)MessageModelHeaderValueStateEnum.None);

        #endregion
    }
}
