using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.CreatePayment;

/// <summary>
/// Create payment endpoint
/// </summary>
public class CreatePaymentEndpoint : Endpoint<CreatePaymentRequest, ApiResponse<CreatePaymentResponse>>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreatePaymentEndpoint> _logger;

    public CreatePaymentEndpoint(IPaymentService paymentService, ILogger<CreatePaymentEndpoint> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/create");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new payment";
            s.Description = "Creates a new payment intent with the specified amount and payment method";
        });
    }

    public override async Task HandleAsync(CreatePaymentRequest req, CancellationToken ct)
    {
        try
        {
            var paymentRequest = new PaymentRequest
            {
                Amount = req.Amount,
                Currency = req.Currency,
                PaymentMethodId = req.PaymentMethodId,
                CustomerId = req.CustomerId,
                Description = req.Description,
                Capture = req.Capture
            };

            var result = await _paymentService.CreatePaymentAsync(paymentRequest);

            var response = new CreatePaymentResponse
            {
                Success = result.Success,
                PaymentId = result.PaymentId,
                Status = result.Status,
                Amount = result.Amount,
                Currency = result.Currency,
                ClientSecret = result.ClientSecret,
                ErrorMessage = result.ErrorMessage
            };

            await SendAsync(new ApiResponse<CreatePaymentResponse>
            {
                Success = result.Success,
                Data = response,
                Message = result.Success ? "Payment created successfully" : "Failed to create payment"
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment");
            await SendAsync(new ApiResponse<CreatePaymentResponse>
            {
                Success = false,
                Data = new CreatePaymentResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the payment"
                },
                Message = "Failed to create payment"
            }, cancellation: ct);
        }
    }
} 