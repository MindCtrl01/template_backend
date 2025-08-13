using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Infrastructure.Identity;

/// <summary>
/// User entity for ASP.NET Core Identity
/// </summary>
public class User : IdentityUser<Guid>
{
    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (computed)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// User's profile picture URL
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// User's date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User's last active timestamp
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// User's account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User's account last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the user account is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// User's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// User's city
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// User's country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// User's postal code
    /// </summary>
    public string? PostalCode { get; set; }

    // Two-Factor Authentication Fields
    /// <summary>
    /// Whether 2FA is enabled for this user
    /// </summary>
    public override bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Google Authenticator secret key
    /// </summary>
    public string? GoogleAuthenticatorSecret { get; set; }

    /// <summary>
    /// Whether Google Authenticator is set up
    /// </summary>
    public bool GoogleAuthenticatorEnabled { get; set; }

    /// <summary>
    /// Email verification OTP
    /// </summary>
    public string? EmailVerificationOTP { get; set; }

    /// <summary>
    /// Email verification OTP expiry
    /// </summary>
    public DateTime? EmailVerificationOTPExpiry { get; set; }

    /// <summary>
    /// Whether email is verified
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// Navigation property for user roles
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Navigation property for refresh tokens
    /// </summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
} 