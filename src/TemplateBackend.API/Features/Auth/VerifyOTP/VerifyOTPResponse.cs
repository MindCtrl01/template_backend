namespace TemplateBackend.API.Features.Auth.VerifyOTP;

/// <summary>
/// Response model for OTP verification
/// </summary>
public class VerifyOTPResponse
{
    /// <summary>
    /// Whether OTP verification was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// JWT access token (if verification successful)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// JWT refresh token (if verification successful)
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Token type
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// User information (if verification successful)
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Message for the user
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// User DTO
/// </summary>
public class UserDto
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's username
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether 2FA is enabled
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Whether email is verified
    /// </summary>
    public bool EmailVerified { get; set; }
} 