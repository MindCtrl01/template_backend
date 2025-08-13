using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace TemplateBackend.API.Features.Auth.Register;

/// <summary>
/// FastEndpoint for user registration
/// </summary>
public class RegisterEndpoint : Endpoint<RegisterRequest, ApiResponse<RegisterResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IOTPService _otpService;
    private readonly ILogger<RegisterEndpoint> _logger;

    public RegisterEndpoint(
        UserManager<User> userManager,
        IJwtService jwtService,
        IEmailService emailService,
        IOTPService otpService,
        ILogger<RegisterEndpoint> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _otpService = otpService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "User registration";
            s.Description = "Registers a new user and sends OTP for verification";
        });
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(req.Email);
            if (existingUser != null)
            {
                await SendAsync(ApiResponse<RegisterResponse>.ErrorResult("Email already registered"), cancellation: ct);
                return;
            }

            // Check if username already exists
            if (!string.IsNullOrEmpty(req.Username))
            {
                var existingUsername = await _userManager.FindByNameAsync(req.Username);
                if (existingUsername != null)
                {
                    await SendAsync(ApiResponse<RegisterResponse>.ErrorResult("Username already taken"), cancellation: ct);
                    return;
                }
            }

            // Create new user
            var user = new User
            {
                UserName = req.Username ?? req.Email,
                Email = req.Email,
                FirstName = req.FirstName,
                LastName = req.LastName,
                PhoneNumber = req.PhoneNumber,
                IsActive = true,
                EmailVerified = false,
                TwoFactorEnabled = req.EnableTwoFactor,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                await SendAsync(ApiResponse<RegisterResponse>.ErrorResult($"Registration failed: {errors}"), cancellation: ct);
                return;
            }

            // Generate and send OTP
            var otp = await _otpService.GenerateOTPAsync(req.Email, "Registration");
            var otpSent = await _otpService.SendOTPAsync(req.Email, otp, "Registration");

            if (!otpSent)
            {
                // If OTP sending fails, delete the user and return error
                await _userManager.DeleteAsync(user);
                await SendAsync(ApiResponse<RegisterResponse>.ErrorResult("Failed to send verification email"), cancellation: ct);
                return;
            }

            // Set up Google Authenticator if 2FA is enabled
            GoogleAuthenticatorSetupDto? googleAuthSetup = null;
            if (req.EnableTwoFactor)
            {
                var secret = _otpService.GenerateGoogleAuthenticatorSecret(req.Email);
                var qrCodeUrl = _otpService.GetGoogleAuthenticatorQRCodeUrl(req.Email, secret);
                var manualEntryKey = _otpService.GetGoogleAuthenticatorManualEntryKey(req.Email, secret);

                user.GoogleAuthenticatorSecret = secret;
                user.GoogleAuthenticatorEnabled = true;
                await _userManager.UpdateAsync(user);

                googleAuthSetup = new GoogleAuthenticatorSetupDto
                {
                    Secret = secret,
                    QRCodeUrl = qrCodeUrl,
                    ManualEntryKey = manualEntryKey
                };
            }

            var response = new RegisterResponse
            {
                AccessToken = string.Empty, // No token until OTP is verified
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
                EmailVerificationRequired = true,
                GoogleAuthenticatorSetup = googleAuthSetup
            };

            await SendAsync(ApiResponse<RegisterResponse>.SuccessResult(response, "Registration successful. Please check your email for verification code."), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email: {Email}", req.Email);
            await SendAsync(ApiResponse<RegisterResponse>.ErrorResult("Registration failed"), cancellation: ct);
        }
    }
} 