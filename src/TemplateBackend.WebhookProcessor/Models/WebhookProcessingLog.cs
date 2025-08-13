namespace TemplateBackend.WebhookProcessor.Models;

/// <summary>
/// Webhook processing log entity for database persistence
/// </summary>
public class WebhookProcessingLog
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
    /// Processing step (e.g., validation, parsing, business_logic)
    /// </summary>
    public string ProcessingStep { get; set; } = string.Empty;

    /// <summary>
    /// Processing status (success, failed, pending)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional data (JSON format)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Processing timestamp
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to WebhookEvent
    /// </summary>
    public virtual WebhookEvent WebhookEvent { get; set; } = null!;
} 