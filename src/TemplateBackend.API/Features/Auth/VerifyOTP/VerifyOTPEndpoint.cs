using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace TemplateBackend.API.Features.Auth.VerifyOTP;

/// <summary>
/// FastEndpoint for OTP verification
/// </summary>
public class VerifyOTPEndpoint : Endpoint<VerifyOTPRequest, ApiResponse<VerifyOTPResponse>>
{
    private readonly IOTPService _otpService;
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly ILogger<VerifyOTPEndpoint> _logger;

    public VerifyOTPEndpoint(
        IOTPService otpService,
        UserManager<User> userManager,
        IJwtService jwtService,
        ILogger<VerifyOTPEndpoint> logger)
    {
        _otpService = otpService;
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/auth/verify-otp");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Verify OTP";
            s.Description = "Verifies OTP for registration, login, or password reset";
        });
    }

    public override async Task HandleAsync(VerifyOTPRequest req, CancellationToken ct)
    {
        try
        {
            // Verify OTP
            var isOTPValid = await _otpService.VerifyOTPAsync(req.Email, req.OTP, req.Purpose);
            if (!isOTPValid)
            {
                await SendAsync(ApiResponse<VerifyOTPResponse>.ErrorResult("Invalid or expired OTP"), cancellation: ct);
                return;
            }

            // Handle different purposes
            var response = new VerifyOTPResponse
            {
                IsSuccess = true,
                Message = "OTP verified successfully"
            };

            switch (req.Purpose.ToLower())
            {
                case "registration":
                    await HandleRegistrationVerification(req.Email, response);
                    break;
                case "login":
                    await HandleLoginVerification(req.Email, response);
                    break;
                case "passwordreset":
                    await HandlePasswordResetVerification(req.Email, response);
                    break;
                default:
                    await SendAsync(ApiResponse<VerifyOTPResponse>.ErrorResult("Invalid purpose"), cancellation: ct);
                    return;
            }

            await SendAsync(ApiResponse<VerifyOTPResponse>.SuccessResult(response, "OTP verified successfully"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP verification failed for {Email}", req.Email);
            await SendAsync(ApiResponse<VerifyOTPResponse>.ErrorResult("OTP verification failed"), cancellation: ct);
        }
    }

    private async Task HandleRegistrationVerification(string email, VerifyOTPResponse response)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            response.IsSuccess = false;
            response.Message = "User not found";
            return;
        }

        // Mark email as verified
        user.EmailVerified = true;
        user.EmailVerificationOTP = null;
        user.EmailVerificationOTPExpiry = null;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var jwtToken = _jwtService.GenerateJwtToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);

        response.AccessToken = jwtToken;
        response.RefreshToken = refreshToken;
        response.ExpiresIn = 3600;
        response.TokenType = "Bearer";
        response.User = new UserDto
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
        };
        response.Message = "Registration completed successfully";
    }

    private async Task HandleLoginVerification(string email, VerifyOTPResponse response)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            response.IsSuccess = false;
            response.Message = "User not found";
            return;
        }

        // Generate tokens
        var jwtToken = _jwtService.GenerateJwtToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);

        response.AccessToken = jwtToken;
        response.RefreshToken = refreshToken;
        response.ExpiresIn = 3600;
        response.TokenType = "Bearer";
        response.User = new UserDto
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
        };
        response.Message = "Login completed successfully";
    }

    private async Task HandlePasswordResetVerification(string email, VerifyOTPResponse response)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            response.IsSuccess = false;
            response.Message = "User not found";
            return;
        }

        // For password reset, we don't generate tokens immediately
        // The user needs to provide a new password
        response.Message = "OTP verified. Please provide a new password.";
    }
} 