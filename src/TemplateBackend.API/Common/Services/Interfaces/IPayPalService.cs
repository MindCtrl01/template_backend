using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// PayPal service interface for payment processing
/// </summary>
public interface IPayPalService
{
    /// <summary>
    /// Creates a PayPal order
    /// </summary>
    /// <param name="request">PayPal order request</param>
    /// <returns>PayPal order response</returns>
    Task<PayPalOrderResponse> CreateOrderAsync(PayPalOrderRequest request);

    /// <summary>
    /// Captures a PayPal payment
    /// </summary>
    /// <param name="orderId">PayPal order ID</param>
    /// <param name="request">Capture request</param>
    /// <returns>PayPal capture response</returns>
    Task<PayPalCaptureResponse> CapturePaymentAsync(string orderId, PayPalCaptureRequest request);

    /// <summary>
    /// Gets PayPal order details
    /// </summary>
    /// <param name="orderId">PayPal order ID</param>
    /// <returns>PayPal order details</returns>
    Task<PayPalOrderDetails> GetOrderDetailsAsync(string orderId);

    /// <summary>
    /// Refunds a PayPal payment
    /// </summary>
    /// <param name="captureId">PayPal capture ID</param>
    /// <param name="request">Refund request</param>
    /// <returns>PayPal refund response</returns>
    Task<PayPalRefundResponse> RefundPaymentAsync(string captureId, PayPalRefundRequest request);

    /// <summary>
    /// Gets PayPal access token
    /// </summary>
    /// <returns>Access token</returns>
    Task<string> GetAccessTokenAsync();
}

/// <summary>
/// PayPal order request
/// </summary>
public class PayPalOrderRequest
{
    /// <summary>
    /// Payment intent
    /// </summary>
    public string Intent { get; set; } = "CAPTURE";

    /// <summary>
    /// Purchase units
    /// </summary>
    public List<PayPalPurchaseUnit> PurchaseUnits { get; set; } = new();

    /// <summary>
    /// Application context
    /// </summary>
    public PayPalApplicationContext? ApplicationContext { get; set; }

    /// <summary>
    /// Payment source
    /// </summary>
    public PayPalPaymentSource? PaymentSource { get; set; }
}

/// <summary>
/// PayPal purchase unit
/// </summary>
public class PayPalPurchaseUnit
{
    /// <summary>
    /// Reference ID
    /// </summary>
    public string ReferenceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Amount
    /// </summary>
    public PayPalAmount Amount { get; set; } = new();

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Custom ID
    /// </summary>
    public string? CustomId { get; set; }

    /// <summary>
    /// Invoice ID
    /// </summary>
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Soft descriptor
    /// </summary>
    public string? SoftDescriptor { get; set; }

    /// <summary>
    /// Items
    /// </summary>
    public List<PayPalItem>? Items { get; set; }

    /// <summary>
    /// Shipping
    /// </summary>
    public PayPalShipping? Shipping { get; set; }
}

/// <summary>
/// PayPal amount
/// </summary>
public class PayPalAmount
{
    /// <summary>
    /// Currency code
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Breakdown
    /// </summary>
    public PayPalBreakdown? Breakdown { get; set; }
}

/// <summary>
/// PayPal breakdown
/// </summary>
public class PayPalBreakdown
{
    /// <summary>
    /// Item total
    /// </summary>
    public PayPalMoney? ItemTotal { get; set; }

    /// <summary>
    /// Shipping
    /// </summary>
    public PayPalMoney? Shipping { get; set; }

    /// <summary>
    /// Handling
    /// </summary>
    public PayPalMoney? Handling { get; set; }

    /// <summary>
    /// Tax total
    /// </summary>
    public PayPalMoney? TaxTotal { get; set; }

    /// <summary>
    /// Insurance
    /// </summary>
    public PayPalMoney? Insurance { get; set; }

    /// <summary>
    /// Shipping discount
    /// </summary>
    public PayPalMoney? ShippingDiscount { get; set; }

    /// <summary>
    /// Discount
    /// </summary>
    public PayPalMoney? Discount { get; set; }
}

/// <summary>
/// PayPal money
/// </summary>
public class PayPalMoney
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
/// PayPal item
/// </summary>
public class PayPalItem
{
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit amount
    /// </summary>
    public PayPalMoney UnitAmount { get; set; } = new();

    /// <summary>
    /// Tax
    /// </summary>
    public PayPalMoney? Tax { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public string Quantity { get; set; } = "1";

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// SKU
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    public string Category { get; set; } = "DIGITAL_GOODS";
}

/// <summary>
/// PayPal shipping
/// </summary>
public class PayPalShipping
{
    /// <summary>
    /// Name
    /// </summary>
    public PayPalName Name { get; set; } = new();

    /// <summary>
    /// Address
    /// </summary>
    public PayPalAddress Address { get; set; } = new();
}

/// <summary>
/// PayPal name
/// </summary>
public class PayPalName
{
    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// PayPal address
/// </summary>
public class PayPalAddress
{
    /// <summary>
    /// Address line 1
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Address line 2
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Admin area 1
    /// </summary>
    public string AdminArea1 { get; set; } = string.Empty;

    /// <summary>
    /// Admin area 2
    /// </summary>
    public string? AdminArea2 { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country code
    /// </summary>
    public string CountryCode { get; set; } = "US";
}

/// <summary>
/// PayPal application context
/// </summary>
public class PayPalApplicationContext
{
    /// <summary>
    /// Brand name
    /// </summary>
    public string? BrandName { get; set; }

    /// <summary>
    /// Locale
    /// </summary>
    public string Locale { get; set; } = "en-US";

    /// <summary>
    /// Landing page
    /// </summary>
    public string LandingPage { get; set; } = "LOGIN";

    /// <summary>
    /// Shipping preference
    /// </summary>
    public string ShippingPreference { get; set; } = "NO_SHIPPING";

    /// <summary>
    /// User action
    /// </summary>
    public string UserAction { get; set; } = "PAY_NOW";

    /// <summary>
    /// Payment method
    /// </summary>
    public PayPalPaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// Return URL
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Cancel URL
    /// </summary>
    public string? CancelUrl { get; set; }
}

/// <summary>
/// PayPal payment method
/// </summary>
public class PayPalPaymentMethod
{
    /// <summary>
    /// Payer selected
    /// </summary>
    public string PayerSelected { get; set; } = "PAYPAL";

    /// <summary>
    /// Payee preferred
    /// </summary>
    public string PayeePreferred { get; set; } = "IMMEDIATE_PAYMENT_REQUIRED";
}

/// <summary>
/// PayPal payment source
/// </summary>
public class PayPalPaymentSource
{
    /// <summary>
    /// PayPal
    /// </summary>
    public PayPalPaymentSourcePayPal? PayPal { get; set; }
}

/// <summary>
/// PayPal payment source PayPal
/// </summary>
public class PayPalPaymentSourcePayPal
{
    /// <summary>
    /// Experience context
    /// </summary>
    public PayPalExperienceContext? ExperienceContext { get; set; }
}

/// <summary>
/// PayPal experience context
/// </summary>
public class PayPalExperienceContext
{
    /// <summary>
    /// Payment method preferences
    /// </summary>
    public PayPalPaymentMethodPreferences? PaymentMethodPreferences { get; set; }

    /// <summary>
    /// Payment method selected
    /// </summary>
    public string PaymentMethodSelected { get; set; } = "PAYPAL";

    /// <summary>
    /// Brand name
    /// </summary>
    public string? BrandName { get; set; }

    /// <summary>
    /// Locale
    /// </summary>
    public string Locale { get; set; } = "en-US";

    /// <summary>
    /// Landing page
    /// </summary>
    public string LandingPage { get; set; } = "LOGIN";

    /// <summary>
    /// Shipping preference
    /// </summary>
    public string ShippingPreference { get; set; } = "NO_SHIPPING";

    /// <summary>
    /// User action
    /// </summary>
    public string UserAction { get; set; } = "PAY_NOW";

    /// <summary>
    /// Return URL
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Cancel URL
    /// </summary>
    public string? CancelUrl { get; set; }
}

/// <summary>
/// PayPal payment method preferences
/// </summary>
public class PayPalPaymentMethodPreferences
{
    /// <summary>
    /// Disabled payment method
    /// </summary>
    public string? DisabledPaymentMethod { get; set; }

    /// <summary>
    /// Preferred payment method
    /// </summary>
    public string PreferredPaymentMethod { get; set; } = "PAYPAL";
}

/// <summary>
/// PayPal order response
/// </summary>
public class PayPalOrderResponse
{
    /// <summary>
    /// Order ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Intent
    /// </summary>
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment source
    /// </summary>
    public PayPalPaymentSource? PaymentSource { get; set; }

    /// <summary>
    /// Purchase units
    /// </summary>
    public List<PayPalPurchaseUnit> PurchaseUnits { get; set; } = new();

    /// <summary>
    /// Create time
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Update time
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    public List<PayPalLink> Links { get; set; } = new();
}

/// <summary>
/// PayPal link
/// </summary>
public class PayPalLink
{
    /// <summary>
    /// Href
    /// </summary>
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Rel
    /// </summary>
    public string Rel { get; set; } = string.Empty;

    /// <summary>
    /// Method
    /// </summary>
    public string Method { get; set; } = string.Empty;
}

/// <summary>
/// PayPal capture request
/// </summary>
public class PayPalCaptureRequest
{
    /// <summary>
    /// Note to payer
    /// </summary>
    public string? NoteToPayer { get; set; }

    /// <summary>
    /// Payment instruction
    /// </summary>
    public PayPalPaymentInstruction? PaymentInstruction { get; set; }
}

/// <summary>
/// PayPal payment instruction
/// </summary>
public class PayPalPaymentInstruction
{
    /// <summary>
    /// Disbursement mode
    /// </summary>
    public string DisbursementMode { get; set; } = "INSTANT";

    /// <summary>
    /// Platform fees
    /// </summary>
    public List<PayPalPlatformFee>? PlatformFees { get; set; }
}

/// <summary>
/// PayPal platform fee
/// </summary>
public class PayPalPlatformFee
{
    /// <summary>
    /// Amount
    /// </summary>
    public PayPalMoney Amount { get; set; } = new();

    /// <summary>
    /// Payee
    /// </summary>
    public PayPalPayee? Payee { get; set; }
}

/// <summary>
/// PayPal payee
/// </summary>
public class PayPalPayee
{
    /// <summary>
    /// Email address
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// Merchant ID
    /// </summary>
    public string? MerchantId { get; set; }
}

/// <summary>
/// PayPal capture response
/// </summary>
public class PayPalCaptureResponse
{
    /// <summary>
    /// Capture ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status details
    /// </summary>
    public PayPalStatusDetails? StatusDetails { get; set; }

    /// <summary>
    /// Amount
    /// </summary>
    public PayPalAmount Amount { get; set; } = new();

    /// <summary>
    /// Final capture
    /// </summary>
    public bool FinalCapture { get; set; }

    /// <summary>
    /// Seller protection
    /// </summary>
    public PayPalSellerProtection? SellerProtection { get; set; }

    /// <summary>
    /// Seller receivable breakdown
    /// </summary>
    public PayPalSellerReceivableBreakdown? SellerReceivableBreakdown { get; set; }

    /// <summary>
    /// Create time
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Update time
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    public List<PayPalLink> Links { get; set; } = new();
}

/// <summary>
/// PayPal status details
/// </summary>
public class PayPalStatusDetails
{
    /// <summary>
    /// Reason
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// PayPal seller protection
/// </summary>
public class PayPalSellerProtection
{
    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Dispute categories
    /// </summary>
    public List<string> DisputeCategories { get; set; } = new();
}

/// <summary>
/// PayPal seller receivable breakdown
/// </summary>
public class PayPalSellerReceivableBreakdown
{
    /// <summary>
    /// Gross amount
    /// </summary>
    public PayPalMoney GrossAmount { get; set; } = new();

    /// <summary>
    /// PayPal fee
    /// </summary>
    public PayPalMoney? PayPalFee { get; set; }

    /// <summary>
    /// Net amount
    /// </summary>
    public PayPalMoney NetAmount { get; set; } = new();

    /// <summary>
    /// Receivable amount
    /// </summary>
    public PayPalMoney? ReceivableAmount { get; set; }

    /// <summary>
    /// Exchange rate
    /// </summary>
    public PayPalExchangeRate? ExchangeRate { get; set; }

    /// <summary>
    /// Platform fees
    /// </summary>
    public List<PayPalPlatformFee>? PlatformFees { get; set; }
}

/// <summary>
/// PayPal exchange rate
/// </summary>
public class PayPalExchangeRate
{
    /// <summary>
    /// Source currency
    /// </summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Target currency
    /// </summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Time stamp
    /// </summary>
    public DateTime TimeStamp { get; set; }
}

/// <summary>
/// PayPal order details
/// </summary>
public class PayPalOrderDetails
{
    /// <summary>
    /// Order ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Intent
    /// </summary>
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Payment source
    /// </summary>
    public PayPalPaymentSource? PaymentSource { get; set; }

    /// <summary>
    /// Purchase units
    /// </summary>
    public List<PayPalPurchaseUnit> PurchaseUnits { get; set; } = new();

    /// <summary>
    /// Create time
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Update time
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    public List<PayPalLink> Links { get; set; } = new();
}

/// <summary>
/// PayPal refund request
/// </summary>
public class PayPalRefundRequest
{
    /// <summary>
    /// Amount
    /// </summary>
    public PayPalAmount? Amount { get; set; }

    /// <summary>
    /// Note to payer
    /// </summary>
    public string? NoteToPayer { get; set; }

    /// <summary>
    /// Invoice ID
    /// </summary>
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Reason
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// PayPal refund response
/// </summary>
public class PayPalRefundResponse
{
    /// <summary>
    /// Refund ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status details
    /// </summary>
    public PayPalStatusDetails? StatusDetails { get; set; }

    /// <summary>
    /// Amount
    /// </summary>
    public PayPalAmount Amount { get; set; } = new();

    /// <summary>
    /// Invoice ID
    /// </summary>
    public string? InvoiceId { get; set; }

    /// <summary>
    /// Note to payer
    /// </summary>
    public string? NoteToPayer { get; set; }

    /// <summary>
    /// Seller payable breakdown
    /// </summary>
    public PayPalSellerPayableBreakdown? SellerPayableBreakdown { get; set; }

    /// <summary>
    /// Create time
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// Update time
    /// </summary>
    public DateTime UpdateTime { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    public List<PayPalLink> Links { get; set; } = new();
}

/// <summary>
/// PayPal seller payable breakdown
/// </summary>
public class PayPalSellerPayableBreakdown
{
    /// <summary>
    /// Gross amount
    /// </summary>
    public PayPalMoney GrossAmount { get; set; } = new();

    /// <summary>
    /// PayPal fee
    /// </summary>
    public PayPalMoney? PayPalFee { get; set; }

    /// <summary>
    /// Net amount
    /// </summary>
    public PayPalMoney NetAmount { get; set; } = new();

    /// <summary>
    /// Total refunded amount
    /// </summary>
    public PayPalMoney? TotalRefundedAmount { get; set; }
} 