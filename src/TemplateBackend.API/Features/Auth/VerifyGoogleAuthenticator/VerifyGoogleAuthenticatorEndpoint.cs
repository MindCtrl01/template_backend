using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace TemplateBackend.API.Features.Auth.VerifyGoogleAuthenticator;

/// <summary>
/// FastEndpoint for Google Authenticator verification
/// </summary>
public class VerifyGoogleAuthenticatorEndpoint : Endpoint<VerifyGoogleAuthenticatorRequest, ApiResponse<VerifyGoogleAuthenticatorResponse>>
{
    private readonly IOTPService _otpService;
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<VerifyGoogleAuthenticatorEndpoint> _logger;

    public VerifyGoogleAuthenticatorEndpoint(
        IOTPService otpService,
        UserManager<User> userManager,
        IJwtService jwtService,
        ILogger<VerifyGoogleAuthenticatorEndpoint> logger)
    {
        _otpService = otpService;
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/auth/verify-google-authenticator");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Verify Google Authenticator";
            s.Description = "Verifies TOTP code from Google Authenticator for 2FA";
        });
    }

    public override async Task HandleAsync(VerifyGoogleAuthenticatorRequest req, CancellationToken ct)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null)
            {
                await SendAsync(ApiResponse<VerifyGoogleAuthenticatorResponse>.ErrorResult("User not found"), cancellation: ct);
                return;
            }

            // Check if Google Authenticator is enabled
            if (!user.GoogleAuthenticatorEnabled || string.IsNullOrEmpty(user.GoogleAuthenticatorSecret))
            {
                await SendAsync(ApiResponse<VerifyGoogleAuthenticatorResponse>.ErrorResult("Google Authenticator is not set up for this user"), cancellation: ct);
                return;
            }

            // Verify TOTP
            var isTOTPValid = _otpService.VerifyGoogleAuthenticatorTOTP(user.GoogleAuthenticatorSecret, req.TOTP);
            if (!isTOTPValid)
            {
                await SendAsync(ApiResponse<VerifyGoogleAuthenticatorResponse>.ErrorResult("Invalid TOTP code"), cancellation: ct);
                return;
            }

            // Generate tokens
            var jwtToken = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);

            var response = new VerifyGoogleAuthenticatorResponse
            {
                IsSuccess = true,
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600,
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
                Message = "Google Authenticator verification successful"
            };

            await SendAsync(ApiResponse<VerifyGoogleAuthenticatorResponse>.SuccessResult(response, "2FA verification successful"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Authenticator verification failed for {Email}", req.Email);
            await SendAsync(ApiResponse<VerifyGoogleAuthenticatorResponse>.ErrorResult("Google Authenticator verification failed"), cancellation: ct);
        }
    }
} 