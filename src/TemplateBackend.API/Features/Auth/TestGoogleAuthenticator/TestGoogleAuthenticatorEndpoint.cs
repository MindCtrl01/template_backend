using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Auth.TestGoogleAuthenticator;

/// <summary>
/// FastEndpoint for testing Google Authenticator setup
/// </summary>
public class TestGoogleAuthenticatorEndpoint : Endpoint<TestGoogleAuthenticatorRequest, ApiResponse<TestGoogleAuthenticatorResponse>>
{
    private readonly IOTPService _otpService;
    private readonly ILogger<TestGoogleAuthenticatorEndpoint> _logger;

    public TestGoogleAuthenticatorEndpoint(IOTPService otpService, ILogger<TestGoogleAuthenticatorEndpoint> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/auth/test-google-authenticator");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Test Google Authenticator";
            s.Description = "Generates Google Authenticator setup for testing purposes";
        });
    }

    public override async Task HandleAsync(TestGoogleAuthenticatorRequest req, CancellationToken ct)
    {
        try
        {
            // Generate Google Authenticator secret
            var secret = _otpService.GenerateGoogleAuthenticatorSecret(req.Email);
            
            // Get QR code URL
            var qrCodeUrl = _otpService.GetGoogleAuthenticatorQRCodeUrl(req.Email, secret);
            
            // Get manual entry key
            var manualEntryKey = _otpService.GetGoogleAuthenticatorManualEntryKey(req.Email, secret);
            
            // Get current TOTP for testing
            var currentTOTP = _otpService.GetCurrentTOTP(secret);

            var response = new TestGoogleAuthenticatorResponse
            {
                Secret = secret,
                QRCodeUrl = qrCodeUrl,
                ManualEntryKey = manualEntryKey,
                CurrentTOTP = currentTOTP,
                Message = "Google Authenticator setup generated successfully. Use the QR code or manual entry key to set up your authenticator app."
            };

            await SendAsync(ApiResponse<TestGoogleAuthenticatorResponse>.SuccessResult(response, "Google Authenticator setup generated"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Google Authenticator setup for {Email}", req.Email);
            await SendAsync(ApiResponse<TestGoogleAuthenticatorResponse>.ErrorResult("Failed to generate Google Authenticator setup"), cancellation: ct);
        }
    }
} 