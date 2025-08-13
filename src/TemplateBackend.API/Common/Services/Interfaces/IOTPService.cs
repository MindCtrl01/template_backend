namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// OTP service interface
/// </summary>
public interface IOTPService
{
    /// <summary>
    /// Generates a new OTP
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="purpose">Purpose of OTP (Registration, Login, etc.)</param>
    /// <param name="expiryMinutes">Expiry time in minutes</param>
    /// <returns>Generated OTP</returns>
    Task<string> GenerateOTPAsync(string email, string purpose, int expiryMinutes = 5);

    /// <summary>
    /// Verifies an OTP
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="otp">OTP to verify</param>
    /// <param name="purpose">Purpose of OTP</param>
    /// <returns>True if OTP is valid</returns>
    Task<bool> VerifyOTPAsync(string email, string otp, string purpose);

    /// <summary>
    /// Sends OTP via email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="otp">OTP to send</param>
    /// <param name="purpose">Purpose of OTP</param>
    /// <returns>True if sent successfully</returns>
    Task<bool> SendOTPAsync(string email, string otp, string purpose);

    /// <summary>
    /// Checks if device verification is required for login
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="deviceFingerprint">Device fingerprint</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>True if device verification is required</returns>
    Task<bool> IsDeviceVerificationRequiredAsync(string email, string deviceFingerprint, string ipAddress);

    /// <summary>
    /// Generates and sends device verification OTP
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="deviceFingerprint">Device fingerprint</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>Generated OTP</returns>
    Task<string> GenerateDeviceVerificationOTPAsync(string email, string deviceFingerprint, string ipAddress);

    /// <summary>
    /// Verifies device verification OTP
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="otp">OTP to verify</param>
    /// <param name="deviceFingerprint">Device fingerprint</param>
    /// <returns>True if OTP is valid</returns>
    Task<bool> VerifyDeviceVerificationOTPAsync(string email, string otp, string deviceFingerprint);

    /// <summary>
    /// Saves trusted device information
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="deviceFingerprint">Device fingerprint</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent</param>
    /// <returns>True if saved successfully</returns>
    Task<bool> SaveTrustedDeviceAsync(string email, string deviceFingerprint, string ipAddress, string userAgent);

    /// <summary>
    /// Generates Google Authenticator secret key
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Secret key for Google Authenticator</returns>
    string GenerateGoogleAuthenticatorSecret(string email);

    /// <summary>
    /// Verifies Google Authenticator TOTP
    /// </summary>
    /// <param name="secret">Secret key</param>
    /// <param name="totp">TOTP code</param>
    /// <returns>True if TOTP is valid</returns>
    bool VerifyGoogleAuthenticatorTOTP(string secret, string totp);

    /// <summary>
    /// Gets QR code URL for Google Authenticator
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="secret">Secret key</param>
    /// <param name="issuer">Issuer name</param>
    /// <returns>QR code URL</returns>
    string GetGoogleAuthenticatorQRCodeUrl(string email, string secret, string issuer = "Template Backend");

    /// <summary>
    /// Gets manual entry key for Google Authenticator
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="secret">Secret key</param>
    /// <param name="issuer">Issuer name</param>
    /// <returns>Manual entry key</returns>
    string GetGoogleAuthenticatorManualEntryKey(string email, string secret, string issuer = "Template Backend");

    /// <summary>
    /// Gets current TOTP code for testing purposes
    /// </summary>
    /// <param name="secret">Secret key</param>
    /// <returns>Current TOTP code</returns>
    string GetCurrentTOTP(string secret);
} 