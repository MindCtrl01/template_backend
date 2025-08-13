using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.CreateSubscription;

/// <summary>
/// Create subscription request
/// </summary>
public class CreateSubscriptionRequest
{
    /// <summary>
    /// Customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Price ID
    /// </summary>
    [Required]
    public string PriceId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method ID
    /// </summary>
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Trial end timestamp (optional)
    /// </summary>
    public DateTime? TrialEnd { get; set; }
} 