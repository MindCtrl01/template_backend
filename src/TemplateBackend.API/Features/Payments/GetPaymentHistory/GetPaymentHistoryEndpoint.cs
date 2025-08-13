using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Payments.GetPaymentHistory;

/// <summary>
/// Get payment history endpoint
/// </summary>
public class GetPaymentHistoryEndpoint : Endpoint<GetPaymentHistoryRequest, ApiResponse<GetPaymentHistoryResponse>>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<GetPaymentHistoryEndpoint> _logger;

    public GetPaymentHistoryEndpoint(IPaymentService paymentService, ILogger<GetPaymentHistoryEndpoint> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/payments/history/{UserId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get payment history";
            s.Description = "Retrieves payment history for a specific user with pagination";
            s.ExampleRequest = new GetPaymentHistoryRequest
            {
                UserId = "user123",
                Page = 1,
                PageSize = 10
            };

        });
    }

    public override async Task HandleAsync(GetPaymentHistoryRequest req, CancellationToken ct)
    {
        try
        {
            var (payments, totalCount) = await _paymentService.GetPaymentHistoryAsync(req.UserId, req.Page, req.PageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

            var response = new GetPaymentHistoryResponse
            {
                Payments = payments,
                TotalCount = totalCount,
                Page = req.Page,
                PageSize = req.PageSize,
                TotalPages = totalPages
            };

            await SendAsync(new ApiResponse<GetPaymentHistoryResponse>
            {
                Success = true,
                Data = response,
                Message = "Payment history retrieved successfully"
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment history for user {UserId}", req.UserId);
            await SendAsync(new ApiResponse<GetPaymentHistoryResponse>
            {
                Success = false,
                Data = new GetPaymentHistoryResponse(),
                Message = "Failed to retrieve payment history"
            }, cancellation: ct);
        }
    }
} 