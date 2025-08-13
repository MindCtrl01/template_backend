using TemplateBackend.API.Common.Models;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// Payment service interface for third-party payment integration
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Creates a payment intent
    /// </summary>
    /// <param name="request">Payment creation request</param>
    /// <returns>Payment response</returns>
    Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);

    /// <summary>
    /// Processes a payment
    /// </summary>
    /// <param name="request">Payment processing request</param>
    /// <returns>Payment response</returns>
    Task<PaymentResponse> ProcessPaymentAsync(PaymentProcessRequest request);

    /// <summary>
    /// Captures a payment
    /// </summary>
    /// <param name="request">Payment capture request</param>
    /// <returns>Payment response</returns>
    Task<PaymentResponse> CapturePaymentAsync(PaymentCaptureRequest request);

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="request">Payment refund request</param>
    /// <returns>Payment response</returns>
    Task<PaymentResponse> RefundPaymentAsync(PaymentRefundRequest request);

    /// <summary>
    /// Gets payment details
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment details</returns>
    Task<PaymentDetails> GetPaymentDetailsAsync(string paymentId);

    /// <summary>
    /// Gets payment history for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Payment history</returns>
    Task<(List<PaymentDetails> Payments, int TotalCount)> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Validates payment method
    /// </summary>
    /// <param name="request">Payment method validation request</param>
    /// <returns>Validation result</returns>
    Task<PaymentValidationResult> ValidatePaymentMethodAsync(PaymentMethodValidationRequest request);

    /// <summary>
    /// Creates a subscription
    /// </summary>
    /// <param name="request">Subscription creation request</param>
    /// <returns>Subscription response</returns>
    Task<SubscriptionResponse> CreateSubscriptionAsync(SubscriptionRequest request);

    /// <summary>
    /// Cancels a subscription
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <returns>Subscription response</returns>
    Task<SubscriptionResponse> CancelSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Gets subscription details
    /// </summary>
    /// <param name="subscriptionId">Subscription ID</param>
    /// <returns>Subscription details</returns>
    Task<SubscriptionDetails> GetSubscriptionDetailsAsync(string subscriptionId);
}

/// <summary>
/// Payment request model
/// </summary>
public class PaymentRequest
{
    /// <summary>
    /// Amount in cents
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR)
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Description of the payment
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Metadata for the payment
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Whether to capture the payment immediately
    /// </summary>
    public bool Capture { get; set; } = true;
}

/// <summary>
/// Payment process request model
/// </summary>
public class PaymentProcessRequest
{
    /// <summary>
    /// Payment intent ID
    /// </summary>
    public string PaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
}

/// <summary>
/// Payment capture request model
/// </summary>
public class PaymentCaptureRequest
{
    /// <summary>
    /// Payment intent ID
    /// </summary>
    public string PaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// Amount to capture in cents
    /// </summary>
    public long? Amount { get; set; }
}

/// <summary>
/// Payment refund request model
/// </summary>
public class PaymentRefundRequest
{
    /// <summary>
    /// Payment intent ID
    /// </summary>
    public string PaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// Amount to refund in cents
    /// </summary>
    public long? Amount { get; set; }

    /// <summary>
    /// Reason for refund
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Payment response model
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Payment ID
    /// </summary>
    public string PaymentId { get; set; } = string.Empty;

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
}

/// <summary>
/// Payment method validation request model
/// </summary>
public class PaymentMethodValidationRequest
{
    /// <summary>
    /// Card number
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Expiry month
    /// </summary>
    public int ExpMonth { get; set; }

    /// <summary>
    /// Expiry year
    /// </summary>
    public int ExpYear { get; set; }

    /// <summary>
    /// CVC
    /// </summary>
    public string Cvc { get; set; } = string.Empty;
}

/// <summary>
/// Payment validation result model
/// </summary>
public class PaymentValidationResult
{
    /// <summary>
    /// Whether the payment method is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error message if invalid
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Payment method details if valid
    /// </summary>
    public PaymentMethodDetails? PaymentMethod { get; set; }
}

/// <summary>
/// Subscription request model
/// </summary>
public class SubscriptionRequest
{
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
}

/// <summary>
/// Subscription response model
/// </summary>
public class SubscriptionResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Subscription ID
    /// </summary>
    public string SubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Subscription status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Subscription details model
/// </summary>
public class SubscriptionDetails
{
    /// <summary>
    /// Subscription ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Price ID
    /// </summary>
    public string PriceId { get; set; } = string.Empty;

    /// <summary>
    /// Subscription status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Current period start
    /// </summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>
    /// Current period end
    /// </summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>
    /// Trial end
    /// </summary>
    public DateTime? TrialEnd { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
} 