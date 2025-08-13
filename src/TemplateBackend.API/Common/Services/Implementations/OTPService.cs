using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Google.Authenticator;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// OTP service implementation
/// </summary>
public class OTPService : IOTPService
{
    private readonly IEmailService _emailService;
    private readonly IMongoDBService<OTPDocument> _mongoDBService;
    private readonly IMongoDBService<TrustedDeviceDocument> _trustedDeviceService;
    private readonly ILogger<OTPService> _logger;
    private readonly OTPSettings _otpSettings;
    private readonly TwoFactorAuthenticator _authenticator;

    public OTPService(
        IEmailService emailService,
        IMongoDBService<OTPDocument> mongoDBService,
        IMongoDBService<TrustedDeviceDocument> trustedDeviceService,
        IOptions<OTPSettings> otpSettings,
        ILogger<OTPService> logger)
    {
        _emailService = emailService;
        _mongoDBService = mongoDBService;
        _trustedDeviceService = trustedDeviceService;
        _otpSettings = otpSettings.Value;
        _logger = logger;
        _authenticator = new TwoFactorAuthenticator();
    }

    public async Task<string> GenerateOTPAsync(string email, string purpose, int expiryMinutes = 5)
    {
        try
        {
            // Generate 6-digit OTP
            var otp = GenerateRandomOTP(6);
            var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

            // Save OTP to MongoDB
            var otpDocument = new OTPDocument
            {
                Email = email,
                OTP = otp,
                Purpose = purpose,
                ExpiresAt = expiryTime,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _mongoDBService.CreateAsync(otpDocument);

            _logger.LogInformation("OTP generated for {Email} with purpose {Purpose}", email, purpose);
            return otp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate OTP for {Email}", email);
            throw;
        }
    }

    public async Task<bool> VerifyOTPAsync(string email, string otp, string purpose)
    {
        try
        {
            // Get all OTPs for this email and purpose
            var otpDocuments = await _mongoDBService.FindAsync(doc => 
                doc.Email == email && 
                doc.Purpose == purpose && 
                !doc.IsUsed && 
                doc.ExpiresAt > DateTime.UtcNow);

            var validOTP = otpDocuments.FirstOrDefault(doc => doc.OTP == otp);
            if (validOTP == null)
            {
                _logger.LogWarning("Invalid OTP attempt for {Email} with purpose {Purpose}", email, purpose);
                return false;
            }

            // Mark OTP as used
            validOTP.IsUsed = true;
            validOTP.UsedAt = DateTime.UtcNow;
            await _mongoDBService.UpdateAsync(validOTP.Id, validOTP);

            _logger.LogInformation("OTP verified successfully for {Email} with purpose {Purpose}", email, purpose);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify OTP for {Email}", email);
            return false;
        }
    }

    public async Task<bool> SendOTPAsync(string email, string otp, string purpose)
    {
        try
        {
            var subject = GetOTPEmailSubject(purpose);
            var body = GetOTPEmailBody(otp, purpose);

            var result = await _emailService.SendEmailAsync(email, subject, body);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", email);
            return false;
        }
    }

    public async Task<bool> IsDeviceVerificationRequiredAsync(string email, string deviceFingerprint, string ipAddress)
    {
        try
        {
            // Check if this device is already trusted
            var trustedDevices = await _trustedDeviceService.FindAsync(doc => 
                doc.Email == email && 
                doc.DeviceFingerprint == deviceFingerprint &&
                doc.IsActive);

            var isTrusted = trustedDevices.Any();
            
            if (isTrusted)
            {
                _logger.LogInformation("Device is trusted for {Email}", email);
                return false;
            }

            _logger.LogInformation("Device verification required for {Email} from {IP}", email, ipAddress);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check device verification requirement for {Email}", email);
            return true; // Default to requiring verification on error
        }
    }

    public async Task<string> GenerateDeviceVerificationOTPAsync(string email, string deviceFingerprint, string ipAddress)
    {
        try
        {
            // Generate 6-digit OTP
            var otp = GenerateRandomOTP(6);
            var expiryTime = DateTime.UtcNow.AddMinutes(5);

            // Save device verification OTP to MongoDB
            var otpDocument = new OTPDocument
            {
                Email = email,
                OTP = otp,
                Purpose = "DeviceVerification",
                DeviceFingerprint = deviceFingerprint,
                IpAddress = ipAddress,
                ExpiresAt = expiryTime,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _mongoDBService.CreateAsync(otpDocument);

            // Send OTP via email
            var subject = "New Device Login Verification - Template Backend";
            var body = GetDeviceVerificationEmailBody(otp, ipAddress);
            await _emailService.SendEmailAsync(email, subject, body);

            _logger.LogInformation("Device verification OTP generated for {Email} from {IP}", email, ipAddress);
            return otp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate device verification OTP for {Email}", email);
            throw;
        }
    }

    public async Task<bool> VerifyDeviceVerificationOTPAsync(string email, string otp, string deviceFingerprint)
    {
        try
        {
            // Get device verification OTPs for this email and device
            var otpDocuments = await _mongoDBService.FindAsync(doc => 
                doc.Email == email && 
                doc.Purpose == "DeviceVerification" &&
                doc.DeviceFingerprint == deviceFingerprint &&
                !doc.IsUsed && 
                doc.ExpiresAt > DateTime.UtcNow);

            var validOTP = otpDocuments.FirstOrDefault(doc => doc.OTP == otp);
            if (validOTP == null)
            {
                _logger.LogWarning("Invalid device verification OTP for {Email}", email);
                return false;
            }

            // Mark OTP as used
            validOTP.IsUsed = true;
            validOTP.UsedAt = DateTime.UtcNow;
            await _mongoDBService.UpdateAsync(validOTP.Id, validOTP);

            _logger.LogInformation("Device verification OTP verified successfully for {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify device verification OTP for {Email}", email);
            return false;
        }
    }

    public async Task<bool> SaveTrustedDeviceAsync(string email, string deviceFingerprint, string ipAddress, string userAgent)
    {
        try
        {
            var trustedDevice = new TrustedDeviceDocument
            {
                Email = email,
                DeviceFingerprint = deviceFingerprint,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };

            await _trustedDeviceService.CreateAsync(trustedDevice);

            _logger.LogInformation("Trusted device saved for {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save trusted device for {Email}", email);
            return false;
        }
    }

    public string GenerateGoogleAuthenticatorSecret(string email)
    {
        try
        {
            // Generate a random 32-byte key and convert to Base32
            var key = GenerateRandomKey(20);
            return ConvertToBase32(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Google Authenticator secret for {Email}", email);
            throw;
        }
    }

    public bool VerifyGoogleAuthenticatorTOTP(string secret, string totp)
    {
        try
        {
            // Use Google.Authenticator package to validate TOTP
            return _authenticator.ValidateTwoFactorPIN(secret, totp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Google Authenticator TOTP");
            return false;
        }
    }

    public string GetGoogleAuthenticatorQRCodeUrl(string email, string secret, string issuer = "Template Backend")
    {
        try
        {
            // Use Google.Authenticator package to generate setup code
            var setupCode = _authenticator.GenerateSetupCode(issuer, email, secret, false, 3);
            return setupCode.QrCodeSetupImageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Google Authenticator QR code URL for {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Gets the manual entry key for Google Authenticator
    /// </summary>
    public string GetGoogleAuthenticatorManualEntryKey(string email, string secret, string issuer = "Template Backend")
    {
        try
        {
            var setupCode = _authenticator.GenerateSetupCode(issuer, email, secret, false, 3);
            return setupCode.ManualEntryKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Google Authenticator manual entry key for {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Gets the current TOTP code for testing purposes
    /// </summary>
    public string GetCurrentTOTP(string secret)
    {
        try
        {
            return _authenticator.GetCurrentPIN(secret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current TOTP");
            throw;
        }
    }

    private string GenerateRandomOTP(int length)
    {
        var random = new Random();
        var otp = new StringBuilder();
        
        for (int i = 0; i < length; i++)
        {
            otp.Append(random.Next(0, 10));
        }
        
        return otp.ToString();
    }

    private byte[] GenerateRandomKey(int length)
    {
        var key = new byte[length];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(key);
        }
        return key;
    }

    private string ConvertToBase32(byte[] data)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        var bits = 0;
        var value = 0;

        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;

            while (bits >= 5)
            {
                result.Append(base32Chars[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }

        if (bits > 0)
        {
            result.Append(base32Chars[(value << (5 - bits)) & 31]);
        }

        return result.ToString();
    }

    private string GetOTPEmailSubject(string purpose)
    {
        return purpose switch
        {
            "Registration" => "Email Verification - Template Backend",
            "Login" => "Login Verification - Template Backend",
            "PasswordReset" => "Password Reset Verification - Template Backend",
            _ => "Verification Code - Template Backend"
        };
    }

    private string GetOTPEmailBody(string otp, string purpose)
    {
        var action = purpose switch
        {
            "Registration" => "complete your registration",
            "Login" => "complete your login",
            "PasswordReset" => "reset your password",
            _ => "complete your verification"
        };

        return $@"
            <html>
                <body>
                    <h2>Verification Code</h2>
                    <p>Your verification code is:</p>
                    <h1 style='font-size: 32px; color: #007bff; text-align: center; padding: 20px; background-color: #f8f9fa; border-radius: 8px;'>{otp}</h1>
                    <p>Use this code to {action}.</p>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you didn't request this code, please ignore this email.</p>
                    <p>Best regards,<br>Template Backend Team</p>
                </body>
            </html>";
    }

    private string GetDeviceVerificationEmailBody(string otp, string ipAddress)
    {
        return $@"
            <html>
                <body>
                    <h2>New Device Login Verification</h2>
                    <p>We detected a login attempt from a new device or location.</p>
                    <p><strong>IP Address:</strong> {ipAddress}</p>
                    <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <p>If this was you, please use the verification code below:</p>
                    <h1 style='font-size: 32px; color: #007bff; text-align: center; padding: 20px; background-color: #f8f9fa; border-radius: 8px;'>{otp}</h1>
                    <p>This code will expire in 5 minutes.</p>
                    <p>If you didn't attempt to log in, please ignore this email and consider changing your password.</p>
                    <p>Best regards,<br>Template Backend Team</p>
                </body>
            </html>";
    }
}

/// <summary>
/// OTP settings configuration
/// </summary>
public class OTPSettings
{
    public int DefaultExpiryMinutes { get; set; } = 5;
    public int OTPLength { get; set; } = 6;
    public int MaxAttempts { get; set; } = 3;
}

/// <summary>
/// OTP document for MongoDB storage
/// </summary>
public class OTPDocument
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OTP { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? DeviceFingerprint { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Trusted device document for MongoDB storage
/// </summary>
public class TrustedDeviceDocument
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
} 