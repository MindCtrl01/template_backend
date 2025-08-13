using FastEndpoints;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.CreatePayPalOrder;

/// <summary>
/// PayPal order creation endpoint
/// </summary>
public class CreatePayPalOrderEndpoint : Endpoint<CreatePayPalOrderRequest, CreatePayPalOrderResponse>
{
    private readonly IPayPalService _payPalService;
    private readonly ILogger<CreatePayPalOrderEndpoint> _logger;

    public CreatePayPalOrderEndpoint(
        IPayPalService payPalService,
        ILogger<CreatePayPalOrderEndpoint> logger)
    {
        _payPalService = payPalService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/payments/paypal/create-order");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a PayPal order";
            s.Description = "Creates a new PayPal order for payment processing";
            s.ExampleRequest = new CreatePayPalOrderRequest
            {
                Amount = 29.99m,
                Currency = "USD",
                Description = "Premium subscription",
                CustomerId = "customer_123",
                ReturnUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel"
            };

        });
    }

    public override async Task HandleAsync(CreatePayPalOrderRequest req, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Creating PayPal order for customer {CustomerId} with amount {Amount} {Currency}", 
                req.CustomerId, req.Amount, req.Currency);

            // Create PayPal order request
            var payPalRequest = new PayPalOrderRequest
            {
                Intent = "CAPTURE",
                PurchaseUnits = new List<PayPalPurchaseUnit>
                {
                    new PayPalPurchaseUnit
                    {
                        ReferenceId = Guid.NewGuid().ToString(),
                        Amount = new PayPalAmount
                        {
                            CurrencyCode = req.Currency,
                            Value = req.Amount.ToString("F2")
                        },
                        Description = req.Description,
                        CustomId = req.CustomId,
                        InvoiceId = req.InvoiceId
                    }
                },
                ApplicationContext = new PayPalApplicationContext
                {
                    BrandName = "Template Backend",
                    Locale = "en-US",
                    LandingPage = "LOGIN",
                    ShippingPreference = "NO_SHIPPING",
                    UserAction = "PAY_NOW",
                    ReturnUrl = req.ReturnUrl ?? "https://example.com/success",
                    CancelUrl = req.CancelUrl ?? "https://example.com/cancel"
                }
            };

            // Create PayPal order
            var payPalResponse = await _payPalService.CreateOrderAsync(payPalRequest);

            // Find the checkout URL from links
            var checkoutUrl = payPalResponse.Links
                .FirstOrDefault(l => l.Rel == "approve" && l.Method == "GET")
                ?.Href ?? string.Empty;

            var response = new CreatePayPalOrderResponse
            {
                OrderId = payPalResponse.Id,
                Status = payPalResponse.Status,
                CheckoutUrl = checkoutUrl,
                CreateTime = payPalResponse.CreateTime,
                Amount = req.Amount,
                Currency = req.Currency,
                Description = req.Description,
                CustomerId = req.CustomerId,
                Success = true
            };

            _logger.LogInformation("PayPal order created successfully. Order ID: {OrderId}, Checkout URL: {CheckoutUrl}", 
                response.OrderId, response.CheckoutUrl);

            await SendAsync(response, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal order for customer {CustomerId}", req.CustomerId);

            var errorResponse = new CreatePayPalOrderResponse
            {
                OrderId = string.Empty,
                Status = "FAILED",
                CheckoutUrl = string.Empty,
                CreateTime = DateTime.UtcNow,
                Amount = req.Amount,
                Currency = req.Currency,
                Description = req.Description,
                CustomerId = req.CustomerId,
                Success = false,
                ErrorMessage = ex.Message
            };

            await SendAsync(errorResponse, cancellation: ct);
        }
    }
} 