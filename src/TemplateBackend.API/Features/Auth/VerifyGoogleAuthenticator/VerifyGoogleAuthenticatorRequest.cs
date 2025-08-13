using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Auth.VerifyGoogleAuthenticator;

/// <summary>
/// Request model for Google Authenticator verification
/// </summary>
public class VerifyGoogleAuthenticatorRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// TOTP code from Google Authenticator
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string TOTP { get; set; } = string.Empty;
} 