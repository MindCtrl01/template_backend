using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Auth.Login;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email or username
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Whether to remember the user
    /// </summary>
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// Device fingerprint for device verification
    /// </summary>
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// OTP code for 2FA (if required)
    /// </summary>
    public string? OTP { get; set; }

    /// <summary>
    /// Google Authenticator TOTP code (if 2FA enabled)
    /// </summary>
    public string? TOTP { get; set; }
} 