using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace TemplateBackend.API.Features.Auth.Login;

/// <summary>
/// FastEndpoint for user login
/// </summary>
public class LoginEndpoint : Endpoint<LoginRequest, ApiResponse<LoginResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IOTPService _otpService;
    private readonly ILogger<LoginEndpoint> _logger;

    public LoginEndpoint(
        UserManager<User> userManager,
        IJwtService jwtService,
        IEmailService emailService,
        IOTPService otpService,
        ILogger<LoginEndpoint> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "User login";
            s.Description = "Authenticates a user and returns JWT tokens";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        try
        {
            // Find user by email or username
            var user = await _userManager.FindByEmailAsync(req.Email) 
                ?? await _userManager.FindByNameAsync(req.Email);

            if (user == null)
            {
                await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Invalid email or password"), cancellation: ct);
                return;
            }

            // Check if user is active
            if (!user.IsActive)
            {
                await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Account is deactivated"), cancellation: ct);
                return;
            }

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, req.Password);
            if (!isPasswordValid)
            {
                await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Invalid email or password"), cancellation: ct);
                return;
            }

            // Get IP address and user agent
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            // Check if device verification is required
            var deviceFingerprint = req.DeviceFingerprint ?? GenerateDeviceFingerprint(userAgent, ipAddress);
            var isDeviceVerificationRequired = await _otpService.IsDeviceVerificationRequiredAsync(user.Email!, deviceFingerprint, ipAddress);

            // If device verification is required and no OTP provided, send OTP
            if (isDeviceVerificationRequired && string.IsNullOrEmpty(req.OTP))
            {
                var otp = await _otpService.GenerateDeviceVerificationOTPAsync(user.Email!, deviceFingerprint, ipAddress);

                var response_withOTP = new LoginResponse
                {
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    ExpiresIn = 0,
                    TokenType = string.Empty,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        IsActive = user.IsActive,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        EmailVerified = user.EmailVerified
                    },
                    TwoFactorRequired = true,
                    TwoFactorType = "DeviceVerification",
                    Message = "Device verification required. Please check your email for the verification code."
                };

                await SendAsync(ApiResponse<LoginResponse>.SuccessResult(response_withOTP, "Device verification required"), cancellation: ct);
                return;
            }

            // If device verification OTP is provided, verify it
            if (!string.IsNullOrEmpty(req.OTP) && isDeviceVerificationRequired)
            {
                var isOTPValid = await _otpService.VerifyDeviceVerificationOTPAsync(user.Email!, req.OTP, deviceFingerprint);
                if (!isOTPValid)
                {
                    await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Invalid device verification code"), cancellation: ct);
                    return;
                }

                // Save trusted device
                await _otpService.SaveTrustedDeviceAsync(user.Email!, deviceFingerprint, ipAddress, userAgent);
            }

            // Check if 2FA is enabled
            if (user.TwoFactorEnabled)
            {
                // If TOTP is provided, verify Google Authenticator
                if (!string.IsNullOrEmpty(req.TOTP))
                {
                    if (string.IsNullOrEmpty(user.GoogleAuthenticatorSecret))
                    {
                        await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Google Authenticator not set up"), cancellation: ct);
                        return;
                    }

                    var isTOTPValid = _otpService.VerifyGoogleAuthenticatorTOTP(user.GoogleAuthenticatorSecret, req.TOTP);
                    if (!isTOTPValid)
                    {
                        await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Invalid TOTP code"), cancellation: ct);
                        return;
                    }
                }
                // If no TOTP provided, send OTP for 2FA
                else
                {
                    // Generate and send OTP for 2FA
                    var otp = await _otpService.GenerateOTPAsync(user.Email!, "Login");
                    await _otpService.SendOTPAsync(user.Email!, otp, "Login");

                    var response_with2fa = new LoginResponse
                    {
                        AccessToken = string.Empty,
                        RefreshToken = string.Empty,
                        ExpiresIn = 0,
                        TokenType = string.Empty,
                        User = new UserDto
                        {
                            Id = user.Id,
                            Email = user.Email,
                            UserName = user.UserName,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            FullName = user.FullName,
                            PhoneNumber = user.PhoneNumber,
                            IsActive = user.IsActive,
                            TwoFactorEnabled = user.TwoFactorEnabled,
                            EmailVerified = user.EmailVerified
                        },
                        TwoFactorRequired = true,
                        TwoFactorType = "Email",
                        Message = "Please check your email for the verification code"
                    };

                    await SendAsync(ApiResponse<LoginResponse>.SuccessResult(response_with2fa, "2FA required"), cancellation: ct);
                    return;
                }
            }

            // Generate tokens
            var jwtToken = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                JwtToken = jwtToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };

            // TODO: Save refresh token to database

            var response = new LoginResponse
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                TokenType = "Bearer",
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    EmailVerified = user.EmailVerified
                },
                TwoFactorRequired = false,
                Message = "Login successful"
            };

            // Send email notification asynchronously
            _ = Task.Run(async () =>
            {
                await _emailService.SendLoginNotificationAsync(
                    user.Email ?? string.Empty,
                    user.FullName,
                    DateTime.UtcNow,
                    ipAddress);
            });

            await SendAsync(ApiResponse<LoginResponse>.SuccessResult(response, "Login successful"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user: {Email}", req.Email);
            await SendAsync(ApiResponse<LoginResponse>.ErrorResult("Login failed"), cancellation: ct);
        }
    }

    private string GenerateDeviceFingerprint(string userAgent, string ipAddress)
    {
        // Create a simple device fingerprint based on user agent and IP
        var fingerprint = $"{userAgent}_{ipAddress}";
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint));
            return Convert.ToBase64String(hash);
        }
    }
} 