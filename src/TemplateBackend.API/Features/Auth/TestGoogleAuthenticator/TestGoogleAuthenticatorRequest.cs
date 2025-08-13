using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Auth.TestGoogleAuthenticator;

/// <summary>
/// Request model for testing Google Authenticator
/// </summary>
public class TestGoogleAuthenticatorRequest
{
    /// <summary>
    /// Email address for testing
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
} 