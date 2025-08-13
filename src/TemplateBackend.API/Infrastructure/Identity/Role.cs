using Microsoft.AspNetCore.Identity;

namespace TemplateBackend.API.Infrastructure.Identity;

/// <summary>
/// Role entity for ASP.NET Core Identity with additional properties
/// </summary>
public class Role : IdentityRole<Guid>
{
    /// <summary>
    /// Description of the role
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether the role is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the role was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property for user roles
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public Role()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// User role entity for role-based authorization
/// </summary>
public class UserRole : IdentityUserRole<Guid>
{
    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the role
    /// </summary>
    public virtual Role Role { get; set; } = null!;
} 