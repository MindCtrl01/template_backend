using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.ValidatePaymentMethod;

/// <summary>
/// Validate payment method response
/// </summary>
public class ValidatePaymentMethodResponse
{
    /// <summary>
    /// Whether the payment method is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error message if invalid
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Payment method details if valid
    /// </summary>
    public PaymentMethodDetails? PaymentMethod { get; set; }
} 