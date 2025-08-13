using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TemplateBackend.WebhookProcessor.Services.Interfaces;

namespace TemplateBackend.WebhookProcessor.Controllers;

/// <summary>
/// Webhook controller for receiving webhooks from payment providers
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IWebhookProcessorService _webhookProcessorService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IWebhookProcessorService webhookProcessorService,
        ILogger<WebhookController> logger)
    {
        _webhookProcessorService = webhookProcessorService;
        _logger = logger;
    }

    /// <summary>
    /// Receive Stripe webhook
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        try
        {
            var body = await GetRequestBodyAsync();
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

            // Extract event type from Stripe webhook
            var eventType = ExtractStripeEventType(body);

            var result = await _webhookProcessorService.ProcessWebhookAsync(
                "stripe",
                Guid.NewGuid().ToString(), // Stripe will provide this in the actual event
                eventType,
                body,
                signature,
                sourceIp,
                userAgent);

            if (result.Success)
            {
                return Ok(new { message = "Webhook processed successfully", eventId = result.WebhookEventId });
            }
            else
            {
                return BadRequest(new { message = "Webhook processing failed", error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Receive PayPal webhook
    /// </summary>
    [HttpPost("paypal")]
    public async Task<IActionResult> PayPalWebhook()
    {
        try
        {
            var body = await GetRequestBodyAsync();
            var signature = Request.Headers["Paypal-Signature"].FirstOrDefault();
            var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault();

            // Extract event type from PayPal webhook
            var eventType = ExtractPayPalEventType(body);

            var result = await _webhookProcessorService.ProcessWebhookAsync(
                "paypal",
                Guid.NewGuid().ToString(), // PayPal will provide this in the actual event
                eventType,
                body,
                signature,
                sourceIp,
                userAgent);

            if (result.Success)
            {
                return Ok(new { message = "Webhook processed successfully", eventId = result.WebhookEventId });
            }
            else
            {
                return BadRequest(new { message = "Webhook processing failed", error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal webhook");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get webhook statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] string? provider = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var statistics = await _webhookProcessorService.GetStatisticsAsync(provider, fromDate, toDate);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook statistics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get webhook events
    /// </summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetWebhookEvents(
        [FromQuery] string? provider = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var events = await _webhookProcessorService.GetWebhookEventsAsync(provider, status, page, pageSize);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook events");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Retry failed webhooks manually
    /// </summary>
    [HttpPost("retry")]
    public async Task<IActionResult> RetryFailedWebhooks([FromQuery] int maxRetries = 10)
    {
        try
        {
            var retryCount = await _webhookProcessorService.RetryFailedWebhooksAsync(maxRetries);
            return Ok(new { message = $"Retried {retryCount} failed webhooks", retryCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed webhooks");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get request body as string
    /// </summary>
    private async Task<string> GetRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Extract Stripe event type from webhook payload
    /// </summary>
    private string ExtractStripeEventType(string payload)
    {
        try
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(payload);
            return jsonDoc.RootElement.GetProperty("type").GetString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }

    /// <summary>
    /// Extract PayPal event type from webhook payload
    /// </summary>
    private string ExtractPayPalEventType(string payload)
    {
        try
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(payload);
            return jsonDoc.RootElement.GetProperty("event_type").GetString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }
} 