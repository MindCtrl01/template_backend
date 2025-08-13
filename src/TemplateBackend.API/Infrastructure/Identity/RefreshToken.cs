using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Infrastructure.Identity;

/// <summary>
/// Refresh token entity for JWT token refresh functionality
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The refresh token value
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The JWT token that this refresh token is associated with
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string JwtToken { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the refresh token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Date and time when the refresh token was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The IP address from which the refresh token was created
    /// </summary>
    [MaxLength(50)]
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Date and time when the refresh token was revoked
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// The IP address from which the refresh token was revoked
    /// </summary>
    [MaxLength(50)]
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// The refresh token that replaced this one
    /// </summary>
    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Reason for revoking the refresh token
    /// </summary>
    [MaxLength(200)]
    public string? RevokeReason { get; set; }

    /// <summary>
    /// Foreign key to the user who owns this refresh token
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Indicates whether the refresh token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Indicates whether the refresh token is active (not revoked and not expired)
    /// </summary>
    public bool IsActive => RevokedAt == null && !IsExpired;

    public RefreshToken()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
} 