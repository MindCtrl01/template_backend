namespace TemplateBackend.WebhookProcessor.Models;

/// <summary>
/// Payment event entity for database persistence
/// </summary>
public class PaymentEvent
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to WebhookEvent
    /// </summary>
    public int WebhookEventId { get; set; }

    /// <summary>
    /// Payment ID from the provider
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Payment provider (Stripe, PayPal, etc.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Event type (e.g., payment.captured, payment.refunded)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Payment status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Customer ID
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Order ID
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// Transaction ID
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// Additional metadata (JSON format)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to WebhookEvent
    /// </summary>
    public virtual WebhookEvent WebhookEvent { get; set; } = null!;
} 