namespace TemplateBackend.API.Features.Payments.ProcessPayment;

/// <summary>
/// Process payment response
/// </summary>
public class ProcessPaymentResponse
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
    /// Client secret for payment confirmation
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }
} 