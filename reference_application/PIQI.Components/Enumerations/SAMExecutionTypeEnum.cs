namespace PIQI.Components.Models
{
    /// <summary>
    /// Defines the available execution types for SAM (Scoring and Assessment Module) processing.
    /// </summary>
    public enum SAMExecutionTypeEnum
    {
        /// <summary>
        /// Execution is performed through inference logic built into the system.
        /// </summary>
        INFERENCE = 1,

        /// <summary>
        /// Execution is performed by calling a stored procedure in the database.
        /// </summary>
        STORED_PROCEDURE = 2,

        /// <summary>
        /// Execution is performed by referencing and invoking an external assembly.
        /// </summary>
        ASSEMBLY_REFERENCE = 3,

        /// <summary>
        /// Execution is performed by invoking an external web service.
        /// </summary>
        WEB_SERVICE = 4,

        /// <summary>
        /// Execution is performed in a standalone manner, independent of other systems.
        /// </summary>
        STANDALONE = 5
    }
}
