using Microsoft.Extensions.Options;
using Stripe;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Payment service implementation with Stripe integration
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentSettings _paymentSettings;
    private readonly IMongoDBService<PaymentDocument> _paymentService;
    private readonly IMongoDBService<SubscriptionDocument> _subscriptionService;

    public PaymentService(
        IOptions<PaymentSettings> paymentSettings,
        IMongoDBService<PaymentDocument> paymentService,
        IMongoDBService<SubscriptionDocument> subscriptionService,
        ILogger<PaymentService> logger)
    {
        _paymentSettings = paymentSettings.Value;
        _paymentService = paymentService;
        _subscriptionService = subscriptionService;
        _logger = logger;

        // Configure Stripe
        StripeConfiguration.ApiKey = _paymentSettings.StripeSecretKey;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount,
                Currency = request.Currency.ToLower(),
                Customer = request.CustomerId,
                PaymentMethod = request.PaymentMethodId,
                Description = request.Description,
                Metadata = request.Metadata,
                CaptureMethod = request.Capture ? "automatic" : "manual",
                ConfirmationMethod = "manual"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // Save payment to MongoDB
            var paymentDocument = new PaymentDocument
            {
                PaymentIntentId = paymentIntent.Id,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = paymentIntent.Status,
                Description = request.Description,
                Metadata = request.Metadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentService.CreateAsync(paymentDocument);

            var response = new PaymentResponse
            {
                Success = true,
                PaymentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency,
                ClientSecret = paymentIntent.ClientSecret
            };

            _logger.LogInformation("Payment intent created successfully. ID: {PaymentId}, Amount: {Amount} {Currency}", 
                paymentIntent.Id, paymentIntent.Amount, paymentIntent.Currency);

            return response;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while creating payment intent");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment intent");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to create payment intent"
            };
        }
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentProcessRequest request)
    {
        try
        {
            var service = new PaymentIntentService();
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = request.PaymentMethodId
            };

            var paymentIntent = await service.ConfirmAsync(request.PaymentIntentId, confirmOptions);

            // Update payment in MongoDB
            var paymentDocuments = await _paymentService.FindAsync(doc => doc.PaymentIntentId == request.PaymentIntentId);
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

            _logger.LogInformation("Payment processed successfully. ID: {PaymentId}, Status: {Status}", 
                paymentIntent.Id, paymentIntent.Status);

            return response;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while processing payment");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to process payment"
            };
        }
    }

    public async Task<PaymentResponse> CapturePaymentAsync(PaymentCaptureRequest request)
    {
        try
        {
            var service = new PaymentIntentService();
            var captureOptions = new PaymentIntentCaptureOptions();
            
            if (request.Amount.HasValue)
            {
                captureOptions.AmountToCapture = request.Amount.Value;
            }

            var paymentIntent = await service.CaptureAsync(request.PaymentIntentId, captureOptions);

            // Update payment in MongoDB
            var paymentDocuments = await _paymentService.FindAsync(doc => doc.PaymentIntentId == request.PaymentIntentId);
            var paymentDocument = paymentDocuments.FirstOrDefault();
            if (paymentDocument != null)
            {
                paymentDocument.Status = paymentIntent.Status;
                paymentDocument.UpdatedAt = DateTime.UtcNow;
                await _paymentService.UpdateAsync(paymentDocument.Id, paymentDocument);
            }

            var response = new PaymentResponse
            {
                Success = true,
                PaymentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount,
                Currency = paymentIntent.Currency
            };

            _logger.LogInformation("Payment captured successfully. ID: {PaymentId}, Amount: {Amount}", 
                paymentIntent.Id, paymentIntent.Amount);

            return response;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while capturing payment");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture payment");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to capture payment"
            };
        }
    }

    public async Task<PaymentResponse> RefundPaymentAsync(PaymentRefundRequest request)
    {
        try
        {
            var service = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = request.PaymentIntentId,
                Reason = request.Reason
            };

            if (request.Amount.HasValue)
            {
                refundOptions.Amount = request.Amount.Value;
            }

            var refund = await service.CreateAsync(refundOptions);

            var response = new PaymentResponse
            {
                Success = true,
                PaymentId = refund.PaymentIntentId,
                Status = refund.Status,
                Amount = refund.Amount,
                Currency = refund.Currency
            };

            _logger.LogInformation("Payment refunded successfully. ID: {RefundId}, Amount: {Amount}", 
                refund.Id, refund.Amount);

            return response;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while refunding payment");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refund payment");
            return new PaymentResponse
            {
                Success = false,
                ErrorMessage = "Failed to refund payment"
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
            var options = new SubscriptionCreateOptions
            {
                Customer = request.CustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = request.PriceId }
                },
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Expand = new List<string> { "latest_invoice.payment_intent" }
            };

            if (request.TrialEnd.HasValue)
            {
                options.TrialEnd = request.TrialEnd.Value;
            }

            var service = new SubscriptionService();
            var subscription = await service.CreateAsync(options);

            // Save subscription to MongoDB
            var subscriptionDocument = new SubscriptionDocument
            {
                SubscriptionId = subscription.Id,
                CustomerId = request.CustomerId,
                PriceId = request.PriceId,
                Status = subscription.Status,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                TrialEnd = subscription.TrialEnd.HasValue ? subscription.TrialEnd.Value : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _subscriptionService.CreateAsync(subscriptionDocument);

            var response = new SubscriptionResponse
            {
                Success = true,
                SubscriptionId = subscription.Id,
                Status = subscription.Status
            };

            _logger.LogInformation("Subscription created successfully. ID: {SubscriptionId}, Status: {Status}", 
                subscription.Id, subscription.Status);

            return response;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while creating subscription");
            return new SubscriptionResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create subscription");
            return new SubscriptionResponse
            {
                Success = false,
                ErrorMessage = "Failed to create subscription"
            };
        }
    }

    public async Task<SubscriptionResponse> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            var cancelOptions = new SubscriptionCancelOptions
            {
                Prorate = true
            };

            var subscription = await service.CancelAsync(subscriptionId, cancelOptions);

            // Update subscription in MongoDB
            var subscriptionDocuments = await _subscriptionService.FindAsync(doc => doc.SubscriptionId == subscriptionId);
            var subscriptionDocument = subscriptionDocuments.FirstOrDefault();
            if (subscriptionDocument != null)
            {
                subscriptionDocument.Status = subscription.Status;
                subscriptionDocument.UpdatedAt = DateTime.UtcNow;
                await _subscriptionService.UpdateAsync(subscriptionDocument.Id, subscriptionDocument);
            }

            var response = new SubscriptionResponse
            {
                Success = true,
                SubscriptionId = subscription.Id,
                Status = subscription.Status
            };

            _logger.LogInformation("Subscription cancelled successfully. ID: {SubscriptionId}, Status: {Status}", 
                subscription.Id, subscription.Status);

            return response;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error while cancelling subscription");
            return new SubscriptionResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel subscription");
            return new SubscriptionResponse
            {
                Success = false,
                ErrorMessage = "Failed to cancel subscription"
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
                CurrentPeriodEnd =subscription.CurrentPeriodEnd,
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
}

/// <summary>
/// Payment settings configuration
/// </summary>
public class PaymentSettings
{
    public string StripeSecretKey { get; set; } = string.Empty;
    public string StripePublishableKey { get; set; } = string.Empty;
}

/// <summary>
/// Payment document for MongoDB storage
/// </summary>
public class PaymentDocument
{
    public string Id { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Subscription document for MongoDB storage
/// </summary>
public class SubscriptionDocument
{
    public string Id { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string PriceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? TrialEnd { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 