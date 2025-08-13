using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.ValidatePaymentMethod;

/// <summary>
/// Validate payment method endpoint
/// </summary>
public class ValidatePaymentMethodEndpoint : Endpoint<ValidatePaymentMethodRequest, ApiResponse<ValidatePaymentMethodResponse>>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ValidatePaymentMethodEndpoint> _logger;

    public ValidatePaymentMethodEndpoint(IPaymentService paymentService, ILogger<ValidatePaymentMethodEndpoint> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/validate");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Validate payment method";
            s.Description = "Validates a payment method (credit card) with Stripe";
        });
    }

    public override async Task HandleAsync(ValidatePaymentMethodRequest req, CancellationToken ct)
    {
        try
        {
            var validationRequest = new PaymentMethodValidationRequest
            {
                CardNumber = req.CardNumber,
                ExpMonth = req.ExpMonth,
                ExpYear = req.ExpYear,
                Cvc = req.Cvc
            };

            var result = await _paymentService.ValidatePaymentMethodAsync(validationRequest);

            var response = new ValidatePaymentMethodResponse
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage,
                PaymentMethod = result.PaymentMethod
            };

            await SendAsync(new ApiResponse<ValidatePaymentMethodResponse>
            {
                Success = true,
                Data = response,
                Message = result.IsValid ? "Payment method is valid" : "Payment method is invalid"
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate payment method");
            await SendAsync(new ApiResponse<ValidatePaymentMethodResponse>
            {
                Success = false,
                Data = new ValidatePaymentMethodResponse
                {
                    IsValid = false,
                    ErrorMessage = "An error occurred while validating the payment method"
                },
                Message = "Failed to validate payment method"
            }, cancellation: ct);
        }
    }
} 