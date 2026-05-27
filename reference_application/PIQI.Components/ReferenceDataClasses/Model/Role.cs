namespace PIQI.Components.Models
{
    /// <summary>
    /// Represents a role associated with an entity, including its type and attribute information.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets the descriptive name of the role type.
        /// </summary>
        public string? RoleTypeName { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic identifier for the role type.
        /// </summary>
        public string RoleTypeMnemonic { get; set; } = null!;

        /// <summary>
        /// Gets or sets the descriptive name of the attribute associated with this role.
        /// </summary>
        public string? AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic identifier for the attribute associated with this role.
        /// </summary>
        public string AttributeMnemonic { get; set; } = null!;
    }
}
