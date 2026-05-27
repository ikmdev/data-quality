namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a date value in the message header, supporting original, default, and override values.
    /// </summary>
    public class MessageModelHeaderDate
    {
        #region Properties

        /// <summary>
        /// The original value of the date as provided in the message.
        /// </summary>
        public DateTime? OriginalValue { get; set; }

        /// <summary>
        /// The default value of the date, used if no original or override value is present.
        /// </summary>
        public DateTime? DefaultValue { get; set; }

        /// <summary>
        /// The override value of the date, which takes precedence over the original and default values.
        /// </summary>
        public DateTime? OverrideValue { get; set; }

        /// <summary>
        /// Gets the effective date value, considering override, original, and default values in that order.
        /// </summary>
        public DateTime? Value
        {
            get
            {
                if (this.OverrideValue != null) return this.OverrideValue;
                else if (this.OriginalValue != null) return this.OriginalValue;
                else if (this.DefaultValue != null) return this.DefaultValue;
                else return null;
            }
        }

        /// <summary>
        /// Gets the state of the date value, corresponding to Original, Default, or None.
        /// </summary>
        public int ValueState
        {
            get
            {
                if (this.OverrideValue != null) return (int)MessageModelHeaderValueStateEnum.Original;
                else if (this.OriginalValue != null) return (int)MessageModelHeaderValueStateEnum.Original;
                else if (this.DefaultValue != null) return (int)MessageModelHeaderValueStateEnum.Default;
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
