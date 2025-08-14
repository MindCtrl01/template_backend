using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using TemplateBackend.WebhookProcessor.Data;
using TemplateBackend.WebhookProcessor.Models;
using TemplateBackend.WebhookProcessor.Services.Interfaces;

namespace TemplateBackend.WebhookProcessor.Services.Implementations;

/// <summary>
/// Webhook processor service implementation
/// </summary>
public class WebhookProcessorService : IWebhookProcessorService
{
    private readonly WebhookDbContext _context;
    private readonly ILogger<WebhookProcessorService> _logger;
    private readonly Dictionary<string, IWebhookHandler> _webhookHandlers;

    public WebhookProcessorService(
        WebhookDbContext context,
        ILogger<WebhookProcessorService> logger,
        IEnumerable<IWebhookHandler> webhookHandlers)
    {
        _context = context;
        _logger = logger;
        _webhookHandlers = webhookHandlers.ToDictionary(h => h.Provider, h => h);
    }

    public async Task<WebhookProcessingResult> ProcessWebhookAsync(
        string provider,
        string eventId,
        string eventType,
        string payload,
        string? signature = null,
        string? sourceIp = null,
        string? userAgent = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var webhookEvent = new WebhookEvent
        {
            EventId = eventId,
            Provider = provider,
            EventType = eventType,
            Status = "pending",
            RawPayload = payload,
            Signature = signature,
            SourceIp = sourceIp,
            UserAgent = userAgent,
            AttemptCount = 1
        };

        try
        {
            _logger.LogInformation("Processing webhook event {EventId} from {Provider} of type {EventType}", 
                eventId, provider, eventType);

            // Save webhook event to database
            _context.WebhookEvents.Add(webhookEvent);
            await _context.SaveChangesAsync();

            // Log processing start
            await LogProcessingStepAsync(webhookEvent.Id, "validation", "started");

            // Validate webhook signature
            if (!await ValidateWebhookSignatureAsync(provider, payload, signature))
            {
                webhookEvent.Status = "failed";
                webhookEvent.ErrorMessage = "Invalid webhook signature";
                await UpdateWebhookEventAsync(webhookEvent);
                await LogProcessingStepAsync(webhookEvent.Id, "validation", "failed", "Invalid signature");
                
                return new WebhookProcessingResult
                {
                    Success = false,
                    WebhookEventId = webhookEvent.Id,
                    Message = "Invalid webhook signature",
                    ErrorMessage = "Invalid webhook signature",
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }

            await LogProcessingStepAsync(webhookEvent.Id, "validation", "success");

            // Parse webhook payload
            await LogProcessingStepAsync(webhookEvent.Id, "parsing", "started");
            var parsedPayload = await ParseWebhookPayloadAsync(provider, eventType, payload);
            webhookEvent.ProcessedPayload = parsedPayload;
            await LogProcessingStepAsync(webhookEvent.Id, "parsing", "success");

            // Process webhook with appropriate handler
            await LogProcessingStepAsync(webhookEvent.Id, "business_logic", "started");
            var paymentEvents = await ProcessWebhookWithHandlerAsync(provider, eventType, parsedPayload);
            await LogProcessingStepAsync(webhookEvent.Id, "business_logic", "success");

            // Save payment events
            if (paymentEvents.Any())
            {
                foreach (var paymentEvent in paymentEvents)
                {
                    paymentEvent.WebhookEventId = webhookEvent.Id;
                }
                _context.PaymentEvents.AddRange(paymentEvents);
            }

            // Update webhook event status
            webhookEvent.Status = "completed";
            webhookEvent.ProcessedAt = DateTime.UtcNow;
            await UpdateWebhookEventAsync(webhookEvent);

            stopwatch.Stop();

            _logger.LogInformation("Successfully processed webhook event {EventId} in {Duration}ms", 
                eventId, stopwatch.ElapsedMilliseconds);

            return new WebhookProcessingResult
            {
                Success = true,
                WebhookEventId = webhookEvent.Id,
                Message = "Webhook processed successfully",
                DurationMs = stopwatch.ElapsedMilliseconds,
                PaymentEvents = paymentEvents
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing webhook event {EventId}", eventId);

            webhookEvent.Status = "failed";
            webhookEvent.ErrorMessage = ex.Message;
            webhookEvent.NextRetryAt = CalculateNextRetryTime(webhookEvent.AttemptCount);
            await UpdateWebhookEventAsync(webhookEvent);

            await LogProcessingStepAsync(webhookEvent.Id, "processing", "failed", ex.Message);

            return new WebhookProcessingResult
            {
                Success = false,
                WebhookEventId = webhookEvent.Id,
                Message = "Error processing webhook",
                ErrorMessage = ex.Message,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<int> RetryFailedWebhooksAsync(int maxRetries = 10)
    {
        var failedWebhooks = await _context.WebhookEvents
            .Where(w => w.Status == "failed" && w.AttemptCount < w.MaxAttempts)
            .Where(w => w.NextRetryAt <= DateTime.UtcNow)
            .Take(maxRetries)
            .ToListAsync();

        var retryCount = 0;

        foreach (var webhook in failedWebhooks)
        {
            try
            {
                webhook.AttemptCount++;
                webhook.Status = "pending";
                webhook.NextRetryAt = null;
                webhook.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reprocess the webhook
                var result = await ProcessWebhookAsync(
                    webhook.Provider,
                    webhook.EventId,
                    webhook.EventType,
                    webhook.RawPayload,
                    webhook.Signature,
                    webhook.SourceIp,
                    webhook.UserAgent);

                if (result.Success)
                {
                    retryCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying webhook event {EventId}", webhook.EventId);
            }
        }

        return retryCount;
    }

    public async Task<WebhookStatistics> GetStatisticsAsync(
        string? provider = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.WebhookEvents.AsQueryable();

        if (!string.IsNullOrEmpty(provider))
            query = query.Where(w => w.Provider == provider);

        if (fromDate.HasValue)
            query = query.Where(w => w.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.CreatedAt <= toDate.Value);

        var totalEvents = await query.CountAsync();
        var successfulEvents = await query.CountAsync(w => w.Status == "completed");
        var failedEvents = await query.CountAsync(w => w.Status == "failed");
        var pendingEvents = await query.CountAsync(w => w.Status == "pending");

        var eventsByProvider = await query
            .GroupBy(w => w.Provider)
            .Select(g => new { Provider = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Provider, x => x.Count);

        var eventsByType = await query
            .GroupBy(w => w.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EventType, x => x.Count);

        var avgProcessingTime = await query
            .Where(w => w.ProcessedAt.HasValue)
            .Select(w => (w.ProcessedAt!.Value - w.CreatedAt).TotalMilliseconds)
            .DefaultIfEmpty()
            .FirstOrDefaultAsync();

        return new WebhookStatistics
        {
            TotalEvents = totalEvents,
            SuccessfulEvents = successfulEvents,
            FailedEvents = failedEvents,
            PendingEvents = pendingEvents,
            EventsByProvider = eventsByProvider,
            EventsByType = eventsByType,
            AverageProcessingTimeMs = avgProcessingTime
        };
    }

    public async Task<WebhookEventList> GetWebhookEventsAsync(
        string? provider = null,
        string? status = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.WebhookEvents
            .Include(w => w.ProcessingLogs)
            .Include(w => w.PaymentEvents)
            .AsQueryable();

        if (!string.IsNullOrEmpty(provider))
            query = query.Where(w => w.Provider == provider);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);

        var totalCount = await query.CountAsync();

        var events = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new WebhookEventList
        {
            Events = events,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Validate webhook signature
    /// </summary>
    private async Task<bool> ValidateWebhookSignatureAsync(string provider, string payload, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
            return await Task.FromResult(false);

        // This is a simplified validation - in production, implement proper signature validation
        // for each provider (Stripe, PayPal, etc.)
        return await Task.FromResult(true);
    }

    /// <summary>
    /// Parse webhook payload
    /// </summary>
    private async Task<string> ParseWebhookPayloadAsync(string provider, string eventType, string payload)
    {
        try
        {
            // Parse and validate the JSON payload
            var jsonObject = JsonConvert.DeserializeObject(payload);
            return await Task.FromResult(JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing webhook payload for provider {Provider}", provider);
            throw new InvalidOperationException($"Invalid JSON payload: {ex.Message}");
        }
    }

    /// <summary>
    /// Process webhook with appropriate handler
    /// </summary>
    private async Task<List<PaymentEvent>> ProcessWebhookWithHandlerAsync(string provider, string eventType, string payload)
    {
        if (!_webhookHandlers.TryGetValue(provider, out var handler))
        {
            _logger.LogWarning("No handler found for provider {Provider}", provider);
            return new List<PaymentEvent>();
        }

        return await handler.HandleWebhookAsync(eventType, payload);
    }

    /// <summary>
    /// Update webhook event
    /// </summary>
    private async Task UpdateWebhookEventAsync(WebhookEvent webhookEvent)
    {
        webhookEvent.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Log processing step
    /// </summary>
    private async Task LogProcessingStepAsync(int webhookEventId, string step, string status, string? errorMessage = null)
    {
        var log = new WebhookProcessingLog
        {
            WebhookEventId = webhookEventId,
            ProcessingStep = step,
            Status = status,
            ErrorMessage = errorMessage,
            DurationMs = 0 // This would be calculated in a real implementation
        };

        _context.WebhookProcessingLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Calculate next retry time using exponential backoff
    /// </summary>
    private DateTime CalculateNextRetryTime(int attemptCount)
    {
        var delayMinutes = Math.Min(Math.Pow(2, attemptCount - 1), 60); // Max 60 minutes
        return DateTime.UtcNow.AddMinutes(delayMinutes);
    }
} 