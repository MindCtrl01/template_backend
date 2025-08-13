using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Stripe;
using TemplateBackend.API.Common.Implementations;
using TemplateBackend.API.Common.Interfaces;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.MongoDB;
using System.Text.Json;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Payment queue service with pub-sub pattern and retry policies
/// </summary>
public class PaymentQueueService : IPaymentService
{
    private readonly IMessageQueue _messageQueue;
    private readonly IPaymentErrorService _paymentErrorService;
    private readonly IMongoDBService<PaymentDocument> _paymentService;
    private readonly IMongoDBService<SubscriptionDocument> _subscriptionService;
    private readonly PaymentSettings _paymentSettings;
    private readonly KafkaSettings _kafkaSettings;
    private readonly ILogger<PaymentQueueService> _logger;
    private readonly AsyncRetryPolicy<PaymentResponse> _retryPolicy;

    public PaymentQueueService(
        IMessageQueue messageQueue,
        IPaymentErrorService paymentErrorService,
        IMongoDBService<PaymentDocument> paymentService,
        IMongoDBService<SubscriptionDocument> subscriptionService,
        IOptions<PaymentSettings> paymentSettings,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<PaymentQueueService> logger)
    {
        _messageQueue = messageQueue;
        _paymentErrorService = paymentErrorService;
        _paymentService = paymentService;
        _subscriptionService = subscriptionService;
        _paymentSettings = paymentSettings.Value;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;

        // Configure Stripe
        StripeConfiguration.ApiKey = _paymentSettings.StripeSecretKey;

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy<PaymentResponse>
            .Handle<StripeException>()
            .Or<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Payment operation failed, retrying {RetryCount}/3. Error: {Error}", 
                        retryCount, exception.Exception.Message);
                }
            );
    }

    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
    {
        try
        {
            // Create payment event message
            var paymentEvent = new PaymentEventMessage
            {
                PaymentId = Guid.NewGuid().ToString(),
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethodId = request.PaymentMethodId,
                Description = request.Description,
                Metadata = request.Metadata,
                Capture = request.Capture,
                EventType = PaymentEventType.CreatePayment
            };

            // Publish to payment events topic
            await _messageQueue.PublishAsync(_kafkaSettings.PaymentTopic, paymentEvent, paymentEvent.PaymentId);

            _logger.LogInformation("Payment event published to queue. Payment ID: {PaymentId}, Amount: {Amount} {Currency}", 
                paymentEvent.PaymentId, paymentEvent.Amount, paymentEvent.Currency);

            // Return immediate response indicating the payment is being processed
            return new PaymentResponse
            {
                Success = true,
                PaymentId = paymentEvent.PaymentId,
                Status = "processing",
                Amount = request.Amount,
                Currency = request.Currency
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment event to queue");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to initiate payment processing"
            };
        }
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentProcessRequest request)
    {
        try
        {
            // Create payment event message
            var paymentEvent = new PaymentEventMessage
            {
                PaymentId = request.PaymentIntentId,
                CustomerId = request.CustomerId,
                PaymentMethodId = request.PaymentMethodId,
                EventType = PaymentEventType.ProcessPayment
            };

            // Publish to payment events topic
            await _messageQueue.PublishAsync(_kafkaSettings.PaymentTopic, paymentEvent, paymentEvent.PaymentId);

            _logger.LogInformation("Payment process event published to queue. Payment ID: {PaymentId}", paymentEvent.PaymentId);

            // Return immediate response indicating the payment is being processed
            return new PaymentResponse
            {
                Success = true,
                PaymentId = request.PaymentIntentId,
                Status = "processing"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment process event to queue");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to initiate payment processing"
            };
        }
    }

    public async Task<PaymentResponse> CapturePaymentAsync(PaymentCaptureRequest request)
    {
        try
        {
            // Create payment event message
            var paymentEvent = new PaymentEventMessage
            {
                PaymentId = request.PaymentIntentId,
                Amount = request.Amount ?? 0,
                EventType = PaymentEventType.CapturePayment
            };

            // Publish to payment events topic
            await _messageQueue.PublishAsync(_kafkaSettings.PaymentTopic, paymentEvent, paymentEvent.PaymentId);

            _logger.LogInformation("Payment capture event published to queue. Payment ID: {PaymentId}", paymentEvent.PaymentId);

            // Return immediate response indicating the payment is being processed
            return new PaymentResponse
            {
                Success = true,
                PaymentId = request.PaymentIntentId,
                Status = "processing"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment capture event to queue");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to initiate payment capture"
            };
        }
    }

    public async Task<PaymentResponse> RefundPaymentAsync(PaymentRefundRequest request)
    {
        try
        {
            // Create payment event message
            var paymentEvent = new PaymentEventMessage
            {
                PaymentId = request.PaymentIntentId,
                Amount = request.Amount ?? 0,
                Description = $"Refund: {request.Reason}",
                EventType = PaymentEventType.RefundPayment
            };

            // Publish to payment events topic
            await _messageQueue.PublishAsync(_kafkaSettings.PaymentTopic, paymentEvent, paymentEvent.PaymentId);

            _logger.LogInformation("Payment refund event published to queue. Payment ID: {PaymentId}", paymentEvent.PaymentId);

            // Return immediate response indicating the payment is being processed
            return new PaymentResponse
            {
                Success = true,
                PaymentId = request.PaymentIntentId,
                Status = "processing"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish payment refund event to queue");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to initiate payment refund"
            };
        }
    }

    public async Task<PaymentDetails> GetPaymentDetailsAsync(string paymentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentId);

            var paymentDetails = new PaymentDetails
            {
                Id = paymentIntent.Id,
                CustomerId = paymentIntent.CustomerId,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status,
                Description = paymentIntent.Description,
                CreatedAt = paymentIntent.Created,
                UpdatedAt = DateTime.UtcNow
            };

            if (paymentIntent.PaymentMethod != null)
            {
                paymentDetails.PaymentMethod = new PaymentMethodDetails
                {
                    Id = paymentIntent.PaymentMethod.Id,
                    Type = paymentIntent.PaymentMethod.Type,
                    Brand = paymentIntent.PaymentMethod.Card?.Brand,
                    Last4 = paymentIntent.PaymentMethod.Card?.Last4,
                    ExpMonth = paymentIntent.PaymentMethod.Card?.ExpMonth,
                    ExpYear = paymentIntent.PaymentMethod.Card?.ExpYear
                };
            }

            return paymentDetails;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while getting payment details");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment details");
            throw;
        }
    }

    public async Task<(List<PaymentDetails> Payments, int TotalCount)> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var paymentDocuments = await _paymentService.FindAsync(doc => doc.CustomerId == userId);
            var totalCount = paymentDocuments.Count;
            var paginatedPayments = paymentDocuments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var paymentDetails = new List<PaymentDetails>();
            foreach (var paymentDoc in paginatedPayments)
            {
                try
                {
                    var details = await GetPaymentDetailsAsync(paymentDoc.PaymentIntentId);
                    paymentDetails.Add(details);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get details for payment {PaymentId}", paymentDoc.PaymentIntentId);
                }
            }

            return (paymentDetails, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment history for user {UserId}", userId);
            return (new List<PaymentDetails>(), 0);
        }
    }

    public async Task<PaymentValidationResult> ValidatePaymentMethodAsync(PaymentMethodValidationRequest request)
    {
        try
        {
            // Basic card validation
            if (string.IsNullOrEmpty(request.CardNumber) || request.CardNumber.Length < 13)
            {
                return new PaymentValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid card number"
                };
            }

            if (request.ExpMonth < 1 || request.ExpMonth > 12)
            {
                return new PaymentValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid expiry month"
                };
            }

            if (request.ExpYear < DateTime.Now.Year)
            {
                return new PaymentValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Card has expired"
                };
            }

            if (string.IsNullOrEmpty(request.Cvc) || request.Cvc.Length < 3)
            {
                return new PaymentValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid CVC"
                };
            }

            // Create a test payment method to validate with Stripe
            var options = new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = request.CardNumber,
                    ExpMonth = request.ExpMonth,
                    ExpYear = request.ExpYear,
                    Cvc = request.Cvc
                }
            };

            var service = new PaymentMethodService();
            var paymentMethod = await service.CreateAsync(options);

            var result = new PaymentValidationResult
            {
                IsValid = true,
                PaymentMethod = new PaymentMethodDetails
                {
                    Id = paymentMethod.Id,
                    Type = paymentMethod.Type,
                    Brand = paymentMethod.Card?.Brand,
                    Last4 = paymentMethod.Card?.Last4,
                    ExpMonth = paymentMethod.Card?.ExpMonth,
                    ExpYear = paymentMethod.Card?.ExpYear
                }
            };

            return result;
        }
        catch (StripeException ex)
        {
            return new PaymentValidationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate payment method");
            return new PaymentValidationResult
            {
                IsValid = false,
                ErrorMessage = "Failed to validate payment method"
            };
        }
    }

    public async Task<SubscriptionResponse> CreateSubscriptionAsync(SubscriptionRequest request)
    {
        try
        {
            // Create subscription event message
            var subscriptionEvent = new SubscriptionEventMessage
            {
                CustomerId = request.CustomerId,
                PriceId = request.PriceId,
                PaymentMethodId = request.PaymentMethodId,
                TrialEnd = request.TrialEnd,
                EventType = SubscriptionEventType.CreateSubscription
            };

            // Publish to subscription events topic
            await _messageQueue.PublishAsync(_kafkaSettings.SubscriptionTopic, subscriptionEvent, subscriptionEvent.MessageId);

            _logger.LogInformation("Subscription event published to queue. Customer ID: {CustomerId}, Price ID: {PriceId}", 
                subscriptionEvent.CustomerId, subscriptionEvent.PriceId);

            // Return immediate response indicating the subscription is being processed
            return new SubscriptionResponse
            {
                Success = true,
                SubscriptionId = subscriptionEvent.MessageId,
                Status = "processing"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish subscription event to queue");
            return new SubscriptionResponse
            {
                Success = false,
                ErrorMessage = "Failed to initiate subscription creation"
            };
        }
    }

    public async Task<SubscriptionResponse> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            // Create subscription event message
            var subscriptionEvent = new SubscriptionEventMessage
            {
                SubscriptionId = subscriptionId,
                EventType = SubscriptionEventType.CancelSubscription
            };

            // Publish to subscription events topic
            await _messageQueue.PublishAsync(_kafkaSettings.SubscriptionTopic, subscriptionEvent, subscriptionEvent.MessageId);

            _logger.LogInformation("Subscription cancellation event published to queue. Subscription ID: {SubscriptionId}", subscriptionId);

            // Return immediate response indicating the subscription is being processed
            return new SubscriptionResponse
            {
                Success = true,
                SubscriptionId = subscriptionId,
                Status = "processing"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish subscription cancellation event to queue");
            return new SubscriptionResponse
            {
                Success = false,
                ErrorMessage = "Failed to initiate subscription cancellation"
            };
        }
    }

    public async Task<SubscriptionDetails> GetSubscriptionDetailsAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);

            var subscriptionDetails = new SubscriptionDetails
            {
                Id = subscription.Id,
                CustomerId = subscription.CustomerId,
                PriceId = subscription.Items.Data.FirstOrDefault()?.Price.Id ?? string.Empty,
                Status = subscription.Status,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                TrialEnd = subscription.TrialEnd.HasValue ? subscription.TrialEnd.Value : null,
                CreatedAt = subscription.Created
            };

            return subscriptionDetails;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while getting subscription details");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get subscription details");
            throw;
        }
    }

    /// <summary>
    /// Processes payment events from the queue
    /// </summary>
    /// <param name="paymentEvent">Payment event to process</param>
    /// <returns>Task representing the async operation</returns>
    public async Task ProcessPaymentEventAsync(PaymentEventMessage paymentEvent)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Processing payment event. Payment ID: {PaymentId}, Event Type: {EventType}", 
                paymentEvent.PaymentId, paymentEvent.EventType);

            PaymentResponse result = null!;

            switch (paymentEvent.EventType)
            {
                case PaymentEventType.CreatePayment:
                    result = await ProcessCreatePaymentAsync(paymentEvent);
                    break;
                case PaymentEventType.ProcessPayment:
                    result = await ProcessPaymentProcessingAsync(paymentEvent);
                    break;
                case PaymentEventType.CapturePayment:
                    result = await ProcessPaymentCaptureAsync(paymentEvent);
                    break;
                case PaymentEventType.RefundPayment:
                    result = await ProcessPaymentRefundAsync(paymentEvent);
                    break;
                default:
                    throw new ArgumentException($"Unknown payment event type: {paymentEvent.EventType}");
            }

            stopwatch.Stop();

            // Create payment result message
            var paymentResult = new PaymentResultMessage
            {
                OriginalMessageId = paymentEvent.MessageId,
                PaymentId = result.PaymentId,
                CustomerId = paymentEvent.CustomerId,
                Success = result.Success,
                Status = result.Status,
                Amount = result.Amount,
                Currency = result.Currency,
                ErrorMessage = result.ErrorMessage,
                ClientSecret = result.ClientSecret,
                PaymentMethod = result.PaymentMethod,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };

            // Publish result to payment results topic
            await _messageQueue.PublishAsync(_kafkaSettings.PaymentResultTopic, paymentResult, paymentResult.PaymentId);

            _logger.LogInformation("Payment event processed successfully. Payment ID: {PaymentId}, Status: {Status}, Processing Time: {ProcessingTime}ms", 
                paymentEvent.PaymentId, result.Status, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process payment event. Payment ID: {PaymentId}, Event Type: {EventType}", 
                paymentEvent.PaymentId, paymentEvent.EventType);

            // Save error to MongoDB
            var errorType = ex is StripeException ? PaymentErrorType.StripeApiError : PaymentErrorType.UnknownError;
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

    private async Task<PaymentResponse> ProcessCreatePaymentAsync(PaymentEventMessage paymentEvent)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = paymentEvent.Amount,
                Currency = paymentEvent.Currency.ToLower(),
                Customer = paymentEvent.CustomerId,
                PaymentMethod = paymentEvent.PaymentMethodId,
                Description = paymentEvent.Description,
                Metadata = paymentEvent.Metadata,
                CaptureMethod = paymentEvent.Capture ? "automatic" : "manual",
                ConfirmationMethod = "manual"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // Save payment to MongoDB
            var paymentDocument = new PaymentDocument
            {
                PaymentIntentId = paymentIntent.Id,
                CustomerId = paymentEvent.CustomerId,
                Amount = paymentEvent.Amount,
                Currency = paymentEvent.Currency,
                Status = paymentIntent.Status,
                Description = paymentEvent.Description,
                Metadata = paymentEvent.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentService.CreateAsync(paymentDocument);

            return new PaymentResponse
            {
                Success = true,
                PaymentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                ClientSecret = paymentIntent.ClientSecret
            };
        });
    }

    private async Task<PaymentResponse> ProcessPaymentProcessingAsync(PaymentEventMessage paymentEvent)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var service = new PaymentIntentService();
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentEvent.PaymentMethodId
            };

            var paymentIntent = await service.ConfirmAsync(paymentEvent.PaymentId, confirmOptions);

            // Update payment in MongoDB
            var paymentDocuments = await _paymentService.FindAsync(doc => doc.PaymentIntentId == paymentEvent.PaymentId);
            var paymentDocument = paymentDocuments.FirstOrDefault();
            if (paymentDocument != null)
            {
                paymentDocument.Status = paymentIntent.Status;
                paymentDocument.UpdatedAt = DateTime.UtcNow;
                await _paymentService.UpdateAsync(paymentDocument.Id, paymentDocument);
            }

            var response = new PaymentResponse
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                ClientSecret = paymentIntent.ClientSecret
            };

            if (paymentIntent.Status != "succeeded")
            {
                response.ErrorMessage = paymentIntent.LastPaymentError?.Message;
            }

            return response;
        });
    }

    private async Task<PaymentResponse> ProcessPaymentCaptureAsync(PaymentEventMessage paymentEvent)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var service = new PaymentIntentService();
            var captureOptions = new PaymentIntentCaptureOptions();
            
            if (paymentEvent.Amount > 0)
            {
                captureOptions.AmountToCapture = paymentEvent.Amount;
            }

            var paymentIntent = await service.CaptureAsync(paymentEvent.PaymentId, captureOptions);

            // Update payment in MongoDB
            var paymentDocuments = await _paymentService.FindAsync(doc => doc.PaymentIntentId == paymentEvent.PaymentId);
            var paymentDocument = paymentDocuments.FirstOrDefault();
            if (paymentDocument != null)
            {
                paymentDocument.Status = paymentIntent.Status;
                paymentDocument.UpdatedAt = DateTime.UtcNow;
                await _paymentService.UpdateAsync(paymentDocument.Id, paymentDocument);
            }

            return new PaymentResponse
            {
                Success = true,
                PaymentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency
            };
        });
    }

    private async Task<PaymentResponse> ProcessPaymentRefundAsync(PaymentEventMessage paymentEvent)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var service = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentEvent.PaymentId,
                Reason = "requested_by_customer"
            };

            if (paymentEvent.Amount > 0)
            {
                refundOptions.Amount = paymentEvent.Amount;
            }

            var refund = await service.CreateAsync(refundOptions);

            return new PaymentResponse
            {
                Success = true,
                PaymentId = refund.PaymentIntentId,
                Status = refund.Status,
                Amount = refund.Amount,
                Currency = refund.Currency
            };
        });
    }
} 