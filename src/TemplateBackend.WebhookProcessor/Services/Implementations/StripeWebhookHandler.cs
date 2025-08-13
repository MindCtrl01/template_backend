using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TemplateBackend.WebhookProcessor.Models;
using TemplateBackend.WebhookProcessor.Services.Interfaces;

namespace TemplateBackend.WebhookProcessor.Services.Implementations;

/// <summary>
/// Stripe webhook handler
/// </summary>
public class StripeWebhookHandler : IWebhookHandler
{
    private readonly ILogger<StripeWebhookHandler> _logger;

    public StripeWebhookHandler(ILogger<StripeWebhookHandler> logger)
    {
        _logger = logger;
    }

    public string Provider => "stripe";

    public async Task<List<PaymentEvent>> HandleWebhookAsync(string eventType, string payload)
    {
        _logger.LogInformation("Handling Stripe webhook event: {EventType}", eventType);

        var paymentEvents = new List<PaymentEvent>();

        try
        {
            var stripeEvent = JsonConvert.DeserializeObject<StripeWebhookEvent>(payload);

            switch (eventType)
            {
                case "payment_intent.succeeded":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "payment_intent.succeeded", "completed"));
                    break;

                case "payment_intent.payment_failed":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "payment_intent.payment_failed", "failed"));
                    break;

                case "charge.succeeded":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "charge.succeeded", "completed"));
                    break;

                case "charge.failed":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "charge.failed", "failed"));
                    break;

                case "charge.refunded":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "charge.refunded", "refunded"));
                    break;

                case "invoice.payment_succeeded":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "invoice.payment_succeeded", "completed"));
                    break;

                case "invoice.payment_failed":
                    paymentEvents.Add(CreatePaymentEvent(stripeEvent, "invoice.payment_failed", "failed"));
                    break;

                default:
                    _logger.LogWarning("Unhandled Stripe event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Stripe webhook event: {EventType}", eventType);
            throw;
        }

        return paymentEvents;
    }

    public async Task<bool> ValidateSignatureAsync(string payload, string signature)
    {
        // In production, implement proper Stripe signature validation
        // using the webhook secret
        return !string.IsNullOrEmpty(signature);
    }

    /// <summary>
    /// Create payment event from Stripe webhook
    /// </summary>
    private PaymentEvent CreatePaymentEvent(StripeWebhookEvent stripeEvent, string eventType, string status)
    {
        var paymentEvent = new PaymentEvent
        {
            PaymentId = stripeEvent.Data?.Object?.Id ?? Guid.NewGuid().ToString(),
            Provider = "stripe",
            EventType = eventType,
            Status = status,
            CustomerId = stripeEvent.Data?.Object?.Customer,
            OrderId = stripeEvent.Data?.Object?.Metadata?.GetValueOrDefault("order_id"),
            TransactionId = stripeEvent.Data?.Object?.Id,
            Metadata = JsonConvert.SerializeObject(stripeEvent.Data?.Object?.Metadata)
        };

        // Extract amount and currency if available
        if (stripeEvent.Data?.Object?.Amount != null)
        {
            paymentEvent.Amount = stripeEvent.Data.Object.Amount / 100m; // Convert from cents
            paymentEvent.Currency = stripeEvent.Data.Object.Currency?.ToUpper();
        }

        return paymentEvent;
    }
}

/// <summary>
/// Stripe webhook event model
/// </summary>
public class StripeWebhookEvent
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("object")]
    public string Object { get; set; } = string.Empty;

    [JsonProperty("api_version")]
    public string ApiVersion { get; set; } = string.Empty;

    [JsonProperty("created")]
    public long Created { get; set; }

    [JsonProperty("data")]
    public StripeEventData? Data { get; set; }

    [JsonProperty("livemode")]
    public bool Livemode { get; set; }

    [JsonProperty("pending_webhooks")]
    public int PendingWebhooks { get; set; }

    [JsonProperty("request")]
    public StripeRequest? Request { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Stripe event data
/// </summary>
public class StripeEventData
{
    [JsonProperty("object")]
    public StripeObject? Object { get; set; }
}

/// <summary>
/// Stripe object
/// </summary>
public class StripeObject
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("object")]
    public string Object { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public long? Amount { get; set; }

    [JsonProperty("currency")]
    public string? Currency { get; set; }

    [JsonProperty("customer")]
    public string? Customer { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Stripe request
/// </summary>
public class StripeRequest
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("idempotency_key")]
    public string? IdempotencyKey { get; set; }
} 