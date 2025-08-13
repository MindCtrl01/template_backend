using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.RefundPayPalPayment;

/// <summary>
/// PayPal payment refund request
/// </summary>
public class RefundPayPalPaymentRequest
{
    /// <summary>
    /// PayPal capture ID
    /// </summary>
    [Required]
    public string CaptureId { get; set; } = string.Empty;

    /// <summary>
    /// Refund amount (optional - if not provided, full amount will be refunded)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, GBP)
    /// </summary>
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Note to payer
    /// </summary>
    [StringLength(500, ErrorMessage = "Note to payer cannot exceed 500 characters")]
    public string? NoteToPayer { get; set; }

    /// <summary>
    /// Invoice ID
    /// </summary>
    [StringLength(100, ErrorMessage = "Invoice ID cannot exceed 100 characters")]
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Refund reason
    /// </summary>
    [StringLength(200, ErrorMessage = "Refund reason cannot exceed 200 characters")]
    public string? Reason { get; set; }
} 