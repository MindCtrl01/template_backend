using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.ValidatePaymentMethod;

/// <summary>
/// Validate payment method request
/// </summary>
public class ValidatePaymentMethodRequest
{
    /// <summary>
    /// Card number
    /// </summary>
    [Required]
    [StringLength(19, MinimumLength = 13, ErrorMessage = "Card number must be between 13 and 19 characters")]
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Expiry month (1-12)
    /// </summary>
    [Required]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
    public int ExpMonth { get; set; }

    /// <summary>
    /// Expiry year (4 digits)
    /// </summary>
    [Required]
    [Range(2024, 2030, ErrorMessage = "Expiry year must be between 2024 and 2030")]
    public int ExpYear { get; set; }

    /// <summary>
    /// CVC (3-4 digits)
    /// </summary>
    [Required]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "CVC must be between 3 and 4 characters")]
    public string Cvc { get; set; } = string.Empty;
} 