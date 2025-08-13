using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TemplateBackend.API.Common.Implementations;
using TemplateBackend.API.Common.Interfaces;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Implementations;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.PaymentProcessor;

/// <summary>
/// Payment processor service that consumes messages from Kafka
/// </summary>
public class PaymentProcessorService : BackgroundService
{
    private readonly IMessageQueue _messageQueue;
    private readonly PaymentQueueService _paymentQueueService;
    private readonly IPaymentErrorService _paymentErrorService;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<PaymentProcessorService> _logger;

    public PaymentProcessorService(
        IMessageQueue messageQueue,
        PaymentQueueService paymentQueueService,
        IPaymentErrorService paymentErrorService,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<PaymentProcessorService> logger)
    {
        _messageQueue = messageQueue;
        _paymentQueueService = paymentQueueService;
        _paymentErrorService = paymentErrorService;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Processor Service started");

        try
        {
            // Subscribe to payment events topic
            await _messageQueue.SubscribeAsync<PaymentEventMessage>(
                _kafkaSettings.PaymentTopic,
                "payment-processor-group",
                async paymentEvent => await ProcessPaymentEventAsync(paymentEvent),
                stoppingToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in payment processor service");
        }
    }

    /// <summary>
    /// Processes payment events from the queue
    /// </summary>
    /// <param name="paymentEvent">Payment event to process</param>
    /// <returns>Task representing the async operation</returns>
    private async Task ProcessPaymentEventAsync(PaymentEventMessage paymentEvent)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing payment event. Payment ID: {PaymentId}, Event Type: {EventType}", 
                paymentEvent.PaymentId, paymentEvent.EventType);

            await _paymentQueueService.ProcessPaymentEventAsync(paymentEvent);

            stopwatch.Stop();
            _logger.LogInformation("Payment event processed successfully. Payment ID: {PaymentId}, Processing Time: {ProcessingTime}ms", 
                paymentEvent.PaymentId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process payment event. Payment ID: {PaymentId}, Event Type: {EventType}", 
                paymentEvent.PaymentId, paymentEvent.EventType);

            // Save error to MongoDB
            var errorType = ex is Stripe.StripeException ? PaymentErrorType.StripeApiError : PaymentErrorType.UnknownError;
            var error = PaymentErrorService.CreateFromException(paymentEvent, ex, errorType);
            await _paymentErrorService.SavePaymentErrorAsync(error);

            // Publish retry message if retry count is less than max
            if (paymentEvent.RetryCount < paymentEvent.MaxRetryAttempts)
            {
                var retryMessage = new PaymentRetryMessage
                {
                    OriginalMessageId = paymentEvent.MessageId,
                    PaymentEvent = paymentEvent,
                    ErrorMessage = ex.Message,
                    RetryCount = paymentEvent.RetryCount + 1,
                    RetryDelaySeconds = (int)Math.Pow(2, paymentEvent.RetryCount + 1) * 60 // Exponential backoff in seconds
                };

                await _messageQueue.PublishAsync(_kafkaSettings.RetryTopic, retryMessage, retryMessage.MessageId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment Processor Service stopping...");
        await base.StopAsync(cancellationToken);
    }
} 