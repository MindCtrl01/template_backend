namespace TemplateBackend.API.Features.Auth.TestGoogleAuthenticator;

/// <summary>
/// Response model for testing Google Authenticator
/// </summary>
public class TestGoogleAuthenticatorResponse
{
    /// <summary>
    /// Secret key for Google Authenticator
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// QR code URL for Google Authenticator
    /// </summary>
    public string QRCodeUrl { get; set; } = string.Empty;

    /// <summary>
    /// Manual entry key for Google Authenticator
    /// </summary>
    public string ManualEntryKey { get; set; } = string.Empty;

    /// <summary>
    /// Current TOTP code for testing
    /// </summary>
    public string CurrentTOTP { get; set; } = string.Empty;

    /// <summary>
    /// Message for the user
    /// </summary>
    public string Message { get; set; } = string.Empty;
} 