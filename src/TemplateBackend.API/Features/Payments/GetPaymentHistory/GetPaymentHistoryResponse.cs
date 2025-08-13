using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.GetPaymentHistory;

/// <summary>
/// Get payment history response
/// </summary>
public class GetPaymentHistoryResponse
{
    /// <summary>
    /// List of payments
    /// </summary>
    public List<PaymentDetails> Payments { get; set; } = new();

    /// <summary>
    /// Total count of payments
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
} 