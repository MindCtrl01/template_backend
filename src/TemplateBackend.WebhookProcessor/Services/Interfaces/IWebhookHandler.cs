using TemplateBackend.WebhookProcessor.Models;

namespace TemplateBackend.WebhookProcessor.Services.Interfaces;

/// <summary>
/// Webhook handler interface for different payment providers
/// </summary>
public interface IWebhookHandler
{
    /// <summary>
    /// Provider name (e.g., "stripe", "paypal")
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Handle webhook event
    /// </summary>
    /// <param name="eventType">Event type</param>
    /// <param name="payload">Webhook payload</param>
    /// <returns>List of payment events</returns>
    Task<List<PaymentEvent>> HandleWebhookAsync(string eventType, string payload);

    /// <summary>
    /// Validate webhook signature
    /// </summary>
    /// <param name="payload">Webhook payload</param>
    /// <param name="signature">Webhook signature</param>
    /// <returns>Validation result</returns>
    Task<bool> ValidateSignatureAsync(string payload, string signature);
} 