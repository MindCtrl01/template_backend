using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TemplateBackend.WebhookProcessor.Models;
using TemplateBackend.WebhookProcessor.Services.Interfaces;

namespace TemplateBackend.WebhookProcessor.Services.Implementations;

/// <summary>
/// PayPal webhook handler
/// </summary>
public class PayPalWebhookHandler : IWebhookHandler
{
    private readonly ILogger<PayPalWebhookHandler> _logger;

    public PayPalWebhookHandler(ILogger<PayPalWebhookHandler> logger)
    {
        _logger = logger;
    }

    public string Provider => "paypal";

    public async Task<List<PaymentEvent>> HandleWebhookAsync(string eventType, string payload)
    {
        _logger.LogInformation("Handling PayPal webhook event: {EventType}", eventType);

        var paymentEvents = new List<PaymentEvent>();

        try
        {
            var payPalEvent = JsonConvert.DeserializeObject<PayPalWebhookEvent>(payload);

            switch (eventType)
            {
                case "PAYMENT.CAPTURE.COMPLETED":
                    paymentEvents.Add(CreatePaymentEvent(payPalEvent!, "PAYMENT.CAPTURE.COMPLETED", "completed"));
                    break;

                case "PAYMENT.CAPTURE.DENIED":
                    paymentEvents.Add(CreatePaymentEvent(payPalEvent!, "PAYMENT.CAPTURE.DENIED", "failed"));
                    break;

                case "PAYMENT.CAPTURE.REFUNDED":
                    paymentEvents.Add(CreatePaymentEvent(payPalEvent!, "PAYMENT.CAPTURE.REFUNDED", "refunded"));
                    break;

                case "CHECKOUT.ORDER.APPROVED":
                    paymentEvents.Add(CreatePaymentEvent(payPalEvent!, "CHECKOUT.ORDER.APPROVED", "approved"));
                    break;

                case "CHECKOUT.ORDER.CANCELLED":
                    paymentEvents.Add(CreatePaymentEvent(payPalEvent!, "CHECKOUT.ORDER.CANCELLED", "cancelled"));
                    break;

                case "CHECKOUT.ORDER.COMPLETED":
                    paymentEvents.Add(CreatePaymentEvent(payPalEvent!, "CHECKOUT.ORDER.COMPLETED", "completed"));
                    break;

                default:
                    _logger.LogWarning("Unhandled PayPal event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling PayPal webhook event: {EventType}", eventType);
            throw;
        }

        return paymentEvents;
    }

    public async Task<bool> ValidateSignatureAsync(string payload, string signature)
    {
        // In production, implement proper PayPal signature validation
        // using the webhook secret
        return await Task.FromResult(!string.IsNullOrEmpty(signature));
    }

    /// <summary>
    /// Create payment event from PayPal webhook
    /// </summary>
    private PaymentEvent CreatePaymentEvent(PayPalWebhookEvent payPalEvent, string eventType, string status)
    {
        var paymentEvent = new PaymentEvent
        {
            PaymentId = payPalEvent.Resource?.Id ?? Guid.NewGuid().ToString(),
            Provider = "paypal",
            EventType = eventType,
            Status = status,
            CustomerId = payPalEvent.Resource?.CustomId,
            OrderId = payPalEvent.Resource?.Id,
            TransactionId = payPalEvent.Resource?.Id,
            Metadata = JsonConvert.SerializeObject(payPalEvent.Resource)
        };

        // Extract amount and currency if available
        if (payPalEvent.Resource?.Amount != null)
        {
            if (decimal.TryParse(payPalEvent.Resource.Amount.Value, out var amount))
            {
                paymentEvent.Amount = amount;
            }
            paymentEvent.Currency = payPalEvent.Resource.Amount.CurrencyCode;
        }

        return paymentEvent;
    }
}

/// <summary>
/// PayPal webhook event model
/// </summary>
public class PayPalWebhookEvent
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonProperty("create_time")]
    public DateTime CreateTime { get; set; }

    [JsonProperty("resource_type")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonProperty("resource")]
    public PayPalResource? Resource { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("event_version")]
    public string? EventVersion { get; set; }
}

/// <summary>
/// PayPal resource
/// </summary>
public class PayPalResource
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public PayPalAmount? Amount { get; set; }

    [JsonProperty("custom_id")]
    public string? CustomId { get; set; }

    [JsonProperty("invoice_id")]
    public string? InvoiceId { get; set; }

    [JsonProperty("create_time")]
    public DateTime? CreateTime { get; set; }

    [JsonProperty("update_time")]
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// PayPal amount
/// </summary>
public class PayPalAmount
{
    [JsonProperty("currency_code")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
} 