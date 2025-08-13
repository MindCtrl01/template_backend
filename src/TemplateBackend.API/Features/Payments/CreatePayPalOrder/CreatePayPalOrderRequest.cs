using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.CreatePayPalOrder;

/// <summary>
/// PayPal order creation request
/// </summary>
public class CreatePayPalOrderRequest
{
    /// <summary>
    /// Payment amount
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, GBP)
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Payment description
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Return URL for successful payment
    /// </summary>
    [Url(ErrorMessage = "Return URL must be a valid URL")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Cancel URL for cancelled payment
    /// </summary>
    [Url(ErrorMessage = "Cancel URL must be a valid URL")]
    public string? CancelUrl { get; set; }

    /// <summary>
    /// Custom ID for tracking
    /// </summary>
    [StringLength(100, ErrorMessage = "Custom ID cannot exceed 100 characters")]
    public string? CustomId { get; set; }

    /// <summary>
    /// Invoice ID
    /// </summary>
    [StringLength(100, ErrorMessage = "Invoice ID cannot exceed 100 characters")]
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Payment metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
} 