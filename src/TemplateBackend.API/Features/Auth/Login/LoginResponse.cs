namespace TemplateBackend.API.Features.Auth.Login;

/// <summary>
/// Response model for user login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// JWT refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// User information
    /// </summary>
    public UserDto User { get; set; } = new();

    /// <summary>
    /// Whether 2FA is required
    /// </summary>
    public bool TwoFactorRequired { get; set; }

    /// <summary>
    /// Type of 2FA required (Email, GoogleAuthenticator)
    /// </summary>
    public string? TwoFactorType { get; set; }

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