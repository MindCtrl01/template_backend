using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TemplateBackend.WebhookProcessor.Services.Interfaces;

namespace TemplateBackend.WebhookProcessor;

/// <summary>
/// Webhook processor background service
/// </summary>
public class WebhookProcessorBackgroundService : BackgroundService
{
    private readonly IWebhookProcessorService _webhookProcessorService;
    private readonly ILogger<WebhookProcessorBackgroundService> _logger;
    private readonly WebhookProcessorSettings _settings;

    public WebhookProcessorBackgroundService(
        IWebhookProcessorService webhookProcessorService,
        IOptions<WebhookProcessorSettings> settings,
        ILogger<WebhookProcessorBackgroundService> logger)
    {
        _webhookProcessorService = webhookProcessorService;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Retry failed webhooks
                var retryCount = await _webhookProcessorService.RetryFailedWebhooksAsync(_settings.MaxRetries);
                if (retryCount > 0)
                {
                    _logger.LogInformation("Retried {RetryCount} failed webhooks", retryCount);
                }

                // Get statistics for monitoring
                var statistics = await _webhookProcessorService.GetStatisticsAsync();
                _logger.LogInformation("Webhook Statistics - Total: {Total}, Success: {Success}, Failed: {Failed}, Success Rate: {SuccessRate:F2}%",
                    statistics.TotalEvents, statistics.SuccessfulEvents, statistics.FailedEvents, statistics.SuccessRate);

                // Wait before next iteration
                await Task.Delay(_settings.ProcessingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in webhook processor service");
                await Task.Delay(_settings.ErrorRetryInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Webhook Processor Service stopped");
    }
}

/// <summary>
/// Webhook processor settings
/// </summary>
public class WebhookProcessorSettings
{
    /// <summary>
    /// Processing interval in milliseconds
    /// </summary>
    public int ProcessingInterval { get; set; } = 30000; // 30 seconds

    /// <summary>
    /// Error retry interval in milliseconds
    /// </summary>
    public int ErrorRetryInterval { get; set; } = 5000; // 5 seconds

    /// <summary>
    /// Maximum number of retries per processing cycle
    /// </summary>
    public int MaxRetries { get; set; } = 10;

    /// <summary>
    /// Enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
} 