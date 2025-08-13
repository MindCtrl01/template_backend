using System.Text.Json;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Payment error service implementation
/// </summary>
public class PaymentErrorService : IPaymentErrorService
{
    private readonly IMongoDBService<PaymentErrorDocument> _mongoDBService;
    private readonly ILogger<PaymentErrorService> _logger;

    public PaymentErrorService(IMongoDBService<PaymentErrorDocument> mongoDBService, ILogger<PaymentErrorService> logger)
    {
        _mongoDBService = mongoDBService;
        _logger = logger;
    }

    public async Task SavePaymentErrorAsync(PaymentErrorDocument error)
    {
        try
        {
            await _mongoDBService.CreateAsync(error);
            _logger.LogInformation("Payment error saved successfully. Error ID: {ErrorId}, Payment ID: {PaymentId}", 
                error.Id, error.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save payment error. Payment ID: {PaymentId}", error.PaymentId);
            throw;
        }
    }

    public async Task<List<PaymentErrorDocument>> GetErrorsForRetryAsync(int limit = 100)
    {
        try
        {
            var errors = await _mongoDBService.FindAsync(error => 
                !error.IsResolved && 
                error.RetryCount < error.MaxRetryAttempts && 
                error.RetryAt <= DateTime.UtcNow);

            var sortedErrors = errors
                .OrderBy(e => e.RetryAt)
                .ThenBy(e => e.RetryCount)
                .Take(limit)
                .ToList();

            _logger.LogInformation("Retrieved {Count} payment errors ready for retry", sortedErrors.Count);
            return sortedErrors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment errors for retry");
            return new List<PaymentErrorDocument>();
        }
    }

    public async Task UpdateRetryCountAsync(string errorId, int retryCount)
    {
        try
        {
            var errors = await _mongoDBService.FindAsync(error => error.Id == errorId);
            var error = errors.FirstOrDefault();

            if (error != null)
            {
                error.RetryCount = retryCount;
                error.RetryAt = CalculateNextRetryTime(retryCount);
                await _mongoDBService.UpdateAsync(errorId, error);

                _logger.LogInformation("Updated retry count for error {ErrorId} to {RetryCount}", errorId, retryCount);
            }
            else
            {
                _logger.LogWarning("Payment error not found. Error ID: {ErrorId}", errorId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update retry count for error {ErrorId}", errorId);
            throw;
        }
    }

    public async Task MarkAsResolvedAsync(string errorId, string resolutionMessage)
    {
        try
        {
            var errors = await _mongoDBService.FindAsync(error => error.Id == errorId);
            var error = errors.FirstOrDefault();

            if (error != null)
            {
                error.IsResolved = true;
                error.ResolutionMessage = resolutionMessage;
                error.ResolvedAt = DateTime.UtcNow;
                await _mongoDBService.UpdateAsync(errorId, error);

                _logger.LogInformation("Marked payment error as resolved. Error ID: {ErrorId}, Resolution: {Resolution}", 
                    errorId, resolutionMessage);
            }
            else
            {
                _logger.LogWarning("Payment error not found. Error ID: {ErrorId}", errorId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark error as resolved. Error ID: {ErrorId}", errorId);
            throw;
        }
    }

    public async Task<PaymentErrorStatistics> GetErrorStatisticsAsync()
    {
        try
        {
            var allErrors = await _mongoDBService.GetAllAsync();
            var errors = allErrors.ToList();

            var statistics = new PaymentErrorStatistics
            {
                TotalErrors = errors.Count,
                ResolvedErrors = errors.Count(e => e.IsResolved),
                PendingErrors = errors.Count(e => !e.IsResolved),
                ErrorsReadyForRetry = errors.Count(e => !e.IsResolved && e.RetryCount < e.MaxRetryAttempts && e.RetryAt <= DateTime.UtcNow),
                AverageRetryCount = errors.Any() ? errors.Average(e => e.RetryCount) : 0
            };

            // Calculate error count by type
            statistics.ErrorCountByType = errors
                .GroupBy(e => e.ErrorType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate error count by payment event type
            statistics.ErrorCountByPaymentEventType = errors
                .GroupBy(e => e.PaymentEventType)
                .ToDictionary(g => g.Key, g => g.Count());

            _logger.LogInformation("Retrieved payment error statistics. Total: {Total}, Resolved: {Resolved}, Pending: {Pending}", 
                statistics.TotalErrors, statistics.ResolvedErrors, statistics.PendingErrors);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment error statistics");
            return new PaymentErrorStatistics();
        }
    }

    /// <summary>
    /// Calculates the next retry time based on retry count
    /// </summary>
    /// <param name="retryCount">Current retry count</param>
    /// <returns>Next retry time</returns>
    private DateTime CalculateNextRetryTime(int retryCount)
    {
        // Exponential backoff: 1min, 2min, 4min, 8min, 16min, 30min (max)
        var delayMinutes = Math.Min(Math.Pow(2, retryCount), 30);
        return DateTime.UtcNow.AddMinutes(delayMinutes);
    }

    /// <summary>
    /// Creates a payment error document from an exception
    /// </summary>
    /// <param name="paymentEvent">Payment event that failed</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="errorType">Type of error</param>
    /// <returns>Payment error document</returns>
    public static PaymentErrorDocument CreateFromException(PaymentEventMessage paymentEvent, Exception exception, PaymentErrorType errorType)
    {
        return new PaymentErrorDocument
        {
            OriginalMessageId = paymentEvent.MessageId,
            PaymentId = paymentEvent.PaymentId,
            CustomerId = paymentEvent.CustomerId,
            ErrorMessage = exception.Message,
            ErrorType = errorType,
            PaymentEventType = paymentEvent.EventType,
            RetryCount = paymentEvent.RetryCount,
            MaxRetryAttempts = paymentEvent.MaxRetryAttempts,
            ErrorOccurredAt = DateTime.UtcNow,
            RetryAt = DateTime.UtcNow.AddMinutes(Math.Min(Math.Pow(2, paymentEvent.RetryCount), 30)),
            PaymentEventData = JsonSerializer.Serialize(paymentEvent),
            StackTrace = exception.StackTrace,
            ErrorContext = new Dictionary<string, string>
            {
                ["ExceptionType"] = exception.GetType().Name,
                ["PaymentAmount"] = paymentEvent.Amount.ToString(),
                ["PaymentCurrency"] = paymentEvent.Currency,
                ["PaymentMethodId"] = paymentEvent.PaymentMethodId
            }
        };
    }
} 