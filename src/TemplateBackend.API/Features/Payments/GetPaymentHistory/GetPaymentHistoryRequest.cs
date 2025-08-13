using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Payments.GetPaymentHistory;

/// <summary>
/// Get payment history request
/// </summary>
public class GetPaymentHistoryRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 10, max: 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;
} 