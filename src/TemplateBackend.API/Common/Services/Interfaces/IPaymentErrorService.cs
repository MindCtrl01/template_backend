using TemplateBackend.API.Common.Models;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// Payment error service interface for persisting payment errors
/// </summary>
public interface IPaymentErrorService
{
    /// <summary>
    /// Saves a payment error to MongoDB
    /// </summary>
    /// <param name="error">Payment error to save</param>
    /// <returns>Task representing the async operation</returns>
    Task SavePaymentErrorAsync(PaymentErrorDocument error);

    /// <summary>
    /// Gets payment errors that are ready for retry
    /// </summary>
    /// <param name="limit">Maximum number of errors to retrieve</param>
    /// <returns>List of payment errors ready for retry</returns>
    Task<List<PaymentErrorDocument>> GetErrorsForRetryAsync(int limit = 100);

    /// <summary>
    /// Updates the retry count for a payment error
    /// </summary>
    /// <param name="errorId">Error ID</param>
    /// <param name="retryCount">New retry count</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateRetryCountAsync(string errorId, int retryCount);

    /// <summary>
    /// Marks a payment error as resolved
    /// </summary>
    /// <param name="errorId">Error ID</param>
    /// <param name="resolutionMessage">Resolution message</param>
    /// <returns>Task representing the async operation</returns>
    Task MarkAsResolvedAsync(string errorId, string resolutionMessage);

    /// <summary>
    /// Gets payment error statistics
    /// </summary>
    /// <returns>Payment error statistics</returns>
    Task<PaymentErrorStatistics> GetErrorStatisticsAsync();
}

/// <summary>
/// Payment error document for MongoDB storage
/// </summary>
public class PaymentErrorDocument
{
    /// <summary>
    /// Unique error ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

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
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Error type
    /// </summary>
    public PaymentErrorType ErrorType { get; set; }

    /// <summary>
    /// Payment event type
    /// </summary>
    public PaymentEventType PaymentEventType { get; set; }

    /// <summary>
    /// Retry count
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Whether the error is resolved
    /// </summary>
    public bool IsResolved { get; set; } = false;

    /// <summary>
    /// Resolution message
    /// </summary>
    public string? ResolutionMessage { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime ErrorOccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the error should be retried
    /// </summary>
    public DateTime RetryAt { get; set; }

    /// <summary>
    /// Timestamp when the error was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Payment event data (serialized)
    /// </summary>
    public string PaymentEventData { get; set; } = string.Empty;

    /// <summary>
    /// Stack trace
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Additional error context
    /// </summary>
    public Dictionary<string, string> ErrorContext { get; set; } = new();
}

/// <summary>
/// Payment error types
/// </summary>
public enum PaymentErrorType
{
    /// <summary>
    /// Stripe API error
    /// </summary>
    StripeApiError,

    /// <summary>
    /// Network error
    /// </summary>
    NetworkError,

    /// <summary>
    /// Validation error
    /// </summary>
    ValidationError,

    /// <summary>
    /// Timeout error
    /// </summary>
    TimeoutError,

    /// <summary>
    /// Unknown error
    /// </summary>
    UnknownError
}

/// <summary>
/// Payment error statistics
/// </summary>
public class PaymentErrorStatistics
{
    /// <summary>
    /// Total number of errors
    /// </summary>
    public int TotalErrors { get; set; }

    /// <summary>
    /// Number of resolved errors
    /// </summary>
    public int ResolvedErrors { get; set; }

    /// <summary>
    /// Number of pending errors
    /// </summary>
    public int PendingErrors { get; set; }

    /// <summary>
    /// Number of errors ready for retry
    /// </summary>
    public int ErrorsReadyForRetry { get; set; }

    /// <summary>
    /// Average retry count
    /// </summary>
    public double AverageRetryCount { get; set; }

    /// <summary>
    /// Error count by type
    /// </summary>
    public Dictionary<PaymentErrorType, int> ErrorCountByType { get; set; } = new();

    /// <summary>
    /// Error count by payment event type
    /// </summary>
    public Dictionary<PaymentEventType, int> ErrorCountByPaymentEventType { get; set; } = new();
} 