namespace TemplateBackend.API.Features.Payments.CreatePayPalOrder;

/// <summary>
/// PayPal order creation response
/// </summary>
public class CreatePayPalOrderResponse
{
    /// <summary>
    /// PayPal order ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Order status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// PayPal checkout URL
    /// </summary>
    public string CheckoutUrl { get; set; } = string.Empty;

    /// <summary>
    /// Order creation time
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Payment description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Success flag
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }
} 