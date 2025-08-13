using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.CreateSubscription;

/// <summary>
/// Create subscription endpoint
/// </summary>
public class CreateSubscriptionEndpoint : Endpoint<CreateSubscriptionRequest, ApiResponse<CreateSubscriptionResponse>>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreateSubscriptionEndpoint> _logger;

    public CreateSubscriptionEndpoint(IPaymentService paymentService, ILogger<CreateSubscriptionEndpoint> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/subscriptions/create");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new subscription";
            s.Description = "Creates a new subscription with the specified price and payment method";
            s.ExampleRequest = new CreateSubscriptionRequest
            {
                CustomerId = "cus_1234567890",
                PriceId = "price_1234567890",
                PaymentMethodId = "pm_1234567890"
            };
        });
    }

    public override async Task HandleAsync(CreateSubscriptionRequest req, CancellationToken ct)
    {
        try
        {
            var subscriptionRequest = new SubscriptionRequest
            {
                CustomerId = req.CustomerId,
                PriceId = req.PriceId,
                PaymentMethodId = req.PaymentMethodId,
                TrialEnd = req.TrialEnd
            };

            var result = await _paymentService.CreateSubscriptionAsync(subscriptionRequest);

            var response = new CreateSubscriptionResponse
            {
                Success = result.Success,
                SubscriptionId = result.SubscriptionId,
                Status = result.Status,
                ErrorMessage = result.ErrorMessage
            };

            await SendAsync(new ApiResponse<CreateSubscriptionResponse>
            {
                Success = result.Success,
                Data = response,
                Message = result.Success ? "Subscription created successfully" : "Failed to create subscription"
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create subscription");
            await SendAsync(new ApiResponse<CreateSubscriptionResponse>
            {
                Success = false,
                Data = new CreateSubscriptionResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating the subscription"
                },
                Message = "Failed to create subscription"
            }, cancellation: ct);
        }
    }
} 