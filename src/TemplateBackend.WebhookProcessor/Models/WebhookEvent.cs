namespace TemplateBackend.WebhookProcessor.Models;

/// <summary>
/// Webhook event entity for database persistence
/// </summary>
public class WebhookEvent
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique event ID from the provider
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Payment provider (Stripe, PayPal, etc.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Event type (e.g., payment.captured, order.completed)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Processing status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Raw webhook payload
    /// </summary>
    public string RawPayload { get; set; } = string.Empty;

    /// <summary>
    /// Processed payload (parsed and validated)
    /// </summary>
    public string? ProcessedPayload { get; set; }

    /// <summary>
    /// Webhook signature for validation
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// Source IP address
    /// </summary>
    public string? SourceIp { get; set; }

    /// <summary>
    /// User agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of processing attempts
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Next retry time
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Processing completed timestamp
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Navigation property for processing logs
    /// </summary>
    public virtual ICollection<WebhookProcessingLog> ProcessingLogs { get; set; } = new List<WebhookProcessingLog>();

    /// <summary>
    /// Navigation property for payment events
    /// </summary>
    public virtual ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
} 