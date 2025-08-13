namespace TemplateBackend.API.Features.Payments.RefundPayPalPayment;

/// <summary>
/// PayPal payment refund response
/// </summary>
public class RefundPayPalPaymentResponse
{
    /// <summary>
    /// Refund ID
    /// </summary>
    public string RefundId { get; set; } = string.Empty;

    /// <summary>
    /// Capture ID
    /// </summary>
    public string CaptureId { get; set; } = string.Empty;

    /// <summary>
    /// Refund status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Refunded amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Refund time
    /// </summary>
    public DateTime RefundTime { get; set; }

    /// <summary>
    /// Invoice ID
    /// </summary>
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Note to payer
    /// </summary>
    public string? NoteToPayer { get; set; }

    /// <summary>
    /// Gross amount
    /// </summary>
    public decimal GrossAmount { get; set; }

    /// <summary>
    /// PayPal fee
    /// </summary>
    public decimal? PayPalFee { get; set; }

    /// <summary>
    /// Net amount
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Total refunded amount
    /// </summary>
    public decimal? TotalRefundedAmount { get; set; }

    /// <summary>
    /// Success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }
} 