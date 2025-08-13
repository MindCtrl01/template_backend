using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.ProcessPayment;

/// <summary>
/// Process payment endpoint
/// </summary>
public class ProcessPaymentEndpoint : Endpoint<ProcessPaymentRequest, ApiResponse<ProcessPaymentResponse>>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ProcessPaymentEndpoint> _logger;

    public ProcessPaymentEndpoint(IPaymentService paymentService, ILogger<ProcessPaymentEndpoint> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/process");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Process a payment";
            s.Description = "Processes a payment intent with the specified payment method";
            s.ExampleRequest = new ProcessPaymentRequest
            {
                PaymentIntentId = "pi_1234567890",
                PaymentMethodId = "pm_1234567890",
                CustomerId = "cus_1234567890"
            };

        });
    }

    public override async Task HandleAsync(ProcessPaymentRequest req, CancellationToken ct)
    {
        try
        {
            var paymentRequest = new PaymentProcessRequest
            {
                PaymentIntentId = req.PaymentIntentId,
                PaymentMethodId = req.PaymentMethodId,
                CustomerId = req.CustomerId
            };

            var result = await _paymentService.ProcessPaymentAsync(paymentRequest);

            var response = new ProcessPaymentResponse
            {
                Success = result.Success,
                PaymentId = result.PaymentId,
                Status = result.Status,
                Amount = result.Amount,
                Currency = result.Currency,
                ClientSecret = result.ClientSecret,
                ErrorMessage = result.ErrorMessage
            };

            await SendAsync(new ApiResponse<ProcessPaymentResponse>
            {
                Success = result.Success,
                Data = response,
                Message = result.Success ? "Payment processed successfully" : "Failed to process payment"
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment");
            await SendAsync(new ApiResponse<ProcessPaymentResponse>
            {
                Success = false,
                Data = new ProcessPaymentResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while processing the payment"
                },
                Message = "Failed to process payment"
            }, cancellation: ct);
        }
    }
} 