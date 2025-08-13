using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.CreatePayment;

/// <summary>
/// Create payment request
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>
    /// Amount in cents
    /// </summary>
    [Required]
    [Range(1, long.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public long Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR)
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
    public string Currency { get; set; } = "USD";

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

    /// <summary>
    /// Description of the payment
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether to capture the payment immediately
    /// </summary>
    public bool Capture { get; set; } = true;
} 