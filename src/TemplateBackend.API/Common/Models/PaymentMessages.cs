namespace TemplateBackend.API.Common.Models;

/// <summary>
/// Payment event message
/// </summary>
public class PaymentEventMessage
{
    /// <summary>
    /// Unique message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Payment ID
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount in cents
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Payment description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Payment metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Whether to capture the payment immediately
    /// </summary>
    public bool Capture { get; set; } = true;

    /// <summary>
    /// Event type
    /// </summary>
    public PaymentEventType EventType { get; set; }

    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Retry count
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Payment result message
/// </summary>
public class PaymentResultMessage
{
    /// <summary>
    /// Unique message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Original payment event message ID
    /// </summary>
    public string OriginalMessageId { get; set; } = string.Empty;

    /// <summary>
    /// Payment ID
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the payment was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Payment status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount in cents
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Client secret for payment confirmation
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Payment method details
    /// </summary>
    public PaymentMethodDetails? PaymentMethod { get; set; }

    /// <summary>
    /// Timestamp when the result was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Payment retry message
/// </summary>
public class PaymentRetryMessage
{
    /// <summary>
    /// Unique message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Original payment event message ID
    /// </summary>
    public string OriginalMessageId { get; set; } = string.Empty;

    /// <summary>
    /// Payment event message
    /// </summary>
    public PaymentEventMessage PaymentEvent { get; set; } = new();

    /// <summary>
    /// Error message that caused the retry
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Retry count
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Timestamp when the retry was scheduled
    /// </summary>
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the retry should be attempted
    /// </summary>
    public DateTime RetryAt { get; set; }

    /// <summary>
    /// Retry delay in seconds
    /// </summary>
    public int RetryDelaySeconds { get; set; }
}

/// <summary>
/// Subscription event message
/// </summary>
public class SubscriptionEventMessage
{
    /// <summary>
    /// Unique message ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Subscription ID
    /// </summary>
    public string SubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Price ID
    /// </summary>
    public string PriceId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Trial end timestamp
    /// </summary>
    public DateTime? TrialEnd { get; set; }

    /// <summary>
    /// Event type
    /// </summary>
    public SubscriptionEventType EventType { get; set; }

    /// <summary>
    /// Timestamp when the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Retry count
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Payment event types
/// </summary>
public enum PaymentEventType
{
    /// <summary>
    /// Create payment intent
    /// </summary>
    CreatePayment,

    /// <summary>
    /// Process payment
    /// </summary>
    ProcessPayment,

    /// <summary>
    /// Capture payment
    /// </summary>
    CapturePayment,

    /// <summary>
    /// Refund payment
    /// </summary>
    RefundPayment
}

/// <summary>
/// Subscription event types
/// </summary>
public enum SubscriptionEventType
{
    /// <summary>
    /// Create subscription
    /// </summary>
    CreateSubscription,

    /// <summary>
    /// Cancel subscription
    /// </summary>
    CancelSubscription
}

public class PaymentDetails
{
    /// <summary>
    /// Payment ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Amount in cents
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Payment status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment method details
    /// </summary>
    public PaymentMethodDetails PaymentMethod { get; set; } = new();

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Payment method details
/// </summary>
public class PaymentMethodDetails
{
    /// <summary>
    /// Payment method ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Payment method type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Card brand (for card payments)
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Last 4 digits (for card payments)
    /// </summary>
    public string? Last4 { get; set; }

    /// <summary>
    /// Expiry month (for card payments)
    /// </summary>
    public long? ExpMonth { get; set; }

    /// <summary>
    /// Expiry year (for card payments)
    /// </summary>
    public long? ExpYear { get; set; }
} 