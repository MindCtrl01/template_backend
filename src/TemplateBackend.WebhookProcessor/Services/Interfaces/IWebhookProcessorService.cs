using TemplateBackend.WebhookProcessor.Models;

namespace TemplateBackend.WebhookProcessor.Services.Interfaces;

/// <summary>
/// Webhook processor service interface
/// </summary>
public interface IWebhookProcessorService
{
    /// <summary>
    /// Process a webhook event
    /// </summary>
    /// <param name="provider">Payment provider</param>
    /// <param name="eventId">Event ID</param>
    /// <param name="eventType">Event type</param>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Webhook signature</param>
    /// <param name="sourceIp">Source IP address</param>
    /// <param name="userAgent">User agent</param>
    /// <returns>Processing result</returns>
    Task<WebhookProcessingResult> ProcessWebhookAsync(
        string provider,
        string eventId,
        string eventType,
        string payload,
        string? signature = null,
        string? sourceIp = null,
        string? userAgent = null);

    /// <summary>
    /// Retry failed webhook events
    /// </summary>
    /// <param name="maxRetries">Maximum number of retries</param>
    /// <returns>Number of events retried</returns>
    Task<int> RetryFailedWebhooksAsync(int maxRetries = 10);

    /// <summary>
    /// Get webhook event statistics
    /// </summary>
    /// <param name="provider">Optional provider filter</param>
    /// <param name="fromDate">Optional start date</param>
    /// <param name="toDate">Optional end date</param>
    /// <returns>Webhook statistics</returns>
    Task<WebhookStatistics> GetStatisticsAsync(
        string? provider = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get webhook events for monitoring
    /// </summary>
    /// <param name="provider">Optional provider filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Webhook events</returns>
    Task<WebhookEventList> GetWebhookEventsAsync(
        string? provider = null,
        string? status = null,
        int page = 1,
        int pageSize = 20);
}

/// <summary>
/// Webhook processing result
/// </summary>
public class WebhookProcessingResult
{
    /// <summary>
    /// Success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Webhook event ID
    /// </summary>
    public int WebhookEventId { get; set; }

    /// <summary>
    /// Processing message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Extracted payment events
    /// </summary>
    public List<PaymentEvent> PaymentEvents { get; set; } = new();
}

/// <summary>
/// Webhook statistics
/// </summary>
public class WebhookStatistics
{
    /// <summary>
    /// Total webhook events
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Successful events
    /// </summary>
    public int SuccessfulEvents { get; set; }

    /// <summary>
    /// Failed events
    /// </summary>
    public int FailedEvents { get; set; }

    /// <summary>
    /// Pending events
    /// </summary>
    public int PendingEvents { get; set; }

    /// <summary>
    /// Events by provider
    /// </summary>
    public Dictionary<string, int> EventsByProvider { get; set; } = new();

    /// <summary>
    /// Events by type
    /// </summary>
    public Dictionary<string, int> EventsByType { get; set; } = new();

    /// <summary>
    /// Average processing time in milliseconds
    /// </summary>
    public double AverageProcessingTimeMs { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate => TotalEvents > 0 ? (double)SuccessfulEvents / TotalEvents * 100 : 0;
}

/// <summary>
/// Webhook event list
/// </summary>
public class WebhookEventList
{
    /// <summary>
    /// Webhook events
    /// </summary>
    public List<WebhookEvent> Events { get; set; } = new();

    /// <summary>
    /// Total count
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
} 