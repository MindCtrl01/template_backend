using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Auth.VerifyOTP;

/// <summary>
/// Request model for OTP verification
/// </summary>
public class VerifyOTPRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// OTP code to verify
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string OTP { get; set; } = string.Empty;

    /// <summary>
    /// Purpose of OTP verification
    /// </summary>
    [Required]
    public string Purpose { get; set; } = string.Empty; // Registration, Login, PasswordReset
} 