using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.CapturePayPalPayment;

/// <summary>
/// PayPal payment capture request
/// </summary>
public class CapturePayPalPaymentRequest
{
    /// <summary>
    /// PayPal order ID
    /// </summary>
    [Required]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Note to payer
    /// </summary>
    [StringLength(500, ErrorMessage = "Note to payer cannot exceed 500 characters")]
    public string? NoteToPayer { get; set; }

    /// <summary>
    /// Payment instruction
    /// </summary>
    public PayPalPaymentInstructionRequest? PaymentInstruction { get; set; }
}

/// <summary>
/// PayPal payment instruction request
/// </summary>
public class PayPalPaymentInstructionRequest
{
    /// <summary>
    /// Disbursement mode
    /// </summary>
    public string DisbursementMode { get; set; } = "INSTANT";

    /// <summary>
    /// Platform fees
    /// </summary>
    public List<PayPalPlatformFeeRequest>? PlatformFees { get; set; }
}

/// <summary>
/// PayPal platform fee request
/// </summary>
public class PayPalPlatformFeeRequest
{
    /// <summary>
    /// Amount
    /// </summary>
    public PayPalMoneyRequest Amount { get; set; } = new();

    /// <summary>
    /// Payee
    /// </summary>
    public PayPalPayeeRequest? Payee { get; set; }
}

/// <summary>
/// PayPal money request
/// </summary>
public class PayPalMoneyRequest
{
    /// <summary>
    /// Currency code
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// PayPal payee request
/// </summary>
public class PayPalPayeeRequest
{
    /// <summary>
    /// Email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Merchant ID
    /// </summary>
    public string? MerchantId { get; set; }
} 