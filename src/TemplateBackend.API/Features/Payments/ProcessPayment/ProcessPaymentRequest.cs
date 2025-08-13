using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.ProcessPayment;

/// <summary>
/// Process payment request
/// </summary>
public class ProcessPaymentRequest
{
    /// <summary>
    /// Payment intent ID
    /// </summary>
    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;

    /// <summary>
    /// Payment method ID
    /// </summary>
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
} 