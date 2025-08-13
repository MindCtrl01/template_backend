namespace TemplateBackend.API.Features.Payments.CapturePayPalPayment;

/// <summary>
/// PayPal payment capture response
/// </summary>
public class CapturePayPalPaymentResponse
{
    /// <summary>
    /// Capture ID
    /// </summary>
    public string CaptureId { get; set; } = string.Empty;

    /// <summary>
    /// Order ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Capture status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Captured amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Capture time
    /// </summary>
    public DateTime CaptureTime { get; set; }

    /// <summary>
    /// Final capture flag
    /// </summary>
    public bool FinalCapture { get; set; }

    /// <summary>
    /// Seller protection status
    /// </summary>
    public string SellerProtectionStatus { get; set; } = string.Empty;

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
    /// Success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }
} 