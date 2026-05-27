namespace PIQI.Components.Models
{
    /// <summary>
    /// Root container for a collection of <see cref="ValueList"/> objects.
    /// </summary>
    public class ValueListRoot
    {
        /// <summary>
        /// The library of value lists.
        /// </summary>
        public List<ValueList> ValueLibrary { get; set; }
    }

    /// <summary>
    /// Represents a list of values (codes) associated with a mnemonic.
    /// </summary>
    public class ValueList
    {
        /// <summary>
        /// The unique mnemonic for the value list.
        /// </summary>
        public string Mnemonic { get; set; } = null!;

        /// <summary>
        /// The list of codes in this value list.
        /// </summary>
        public List<CodeList> CodeList { get; set; }
    }
}
