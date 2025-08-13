using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Features.Email.GetEmailStatistics;

/// <summary>
/// Response model for email statistics
/// </summary>
public class GetEmailStatisticsResponse
{
    /// <summary>
    /// Total number of emails
    /// </summary>
    public int TotalEmails { get; set; }

    /// <summary>
    /// Number of sent emails
    /// </summary>
    public int SentEmails { get; set; }

    /// <summary>
    /// Number of failed emails
    /// </summary>
    public int FailedEmails { get; set; }

    /// <summary>
    /// Number of pending emails
    /// </summary>
    public int PendingEmails { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Emails by type
    /// </summary>
    public Dictionary<string, int> EmailsByType { get; set; } = new();

    /// <summary>
    /// Emails by date (last 30 days)
    /// </summary>
    public Dictionary<string, int> EmailsByDate { get; set; } = new();

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// FastEndpoint for getting email statistics
/// </summary>
public class GetEmailStatisticsEndpoint : EndpointWithoutRequest<ApiResponse<GetEmailStatisticsResponse>>
{
    private readonly IEmailMongoService _emailMongoService;
    private readonly ILogger<GetEmailStatisticsEndpoint> _logger;

    public GetEmailStatisticsEndpoint(IEmailMongoService emailMongoService, ILogger<GetEmailStatisticsEndpoint> logger)
    {
        _emailMongoService = emailMongoService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/email/statistics");
        Summary(s =>
        {
            s.Summary = "Get email statistics";
            s.Description = "Returns email statistics from MongoDB";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var statistics = await _emailMongoService.GetEmailStatisticsAsync();

            var response = new GetEmailStatisticsResponse
            {
                TotalEmails = statistics.TotalEmails,
                SentEmails = statistics.SentEmails,
                FailedEmails = statistics.FailedEmails,
                PendingEmails = statistics.PendingEmails,
                SuccessRate = statistics.TotalEmails > 0 
                    ? Math.Round((decimal)statistics.SentEmails / statistics.TotalEmails * 100, 2)
                    : 0,
                EmailsByType = statistics.EmailsByType,
                EmailsByDate = statistics.EmailsByDate,
                LastUpdated = DateTime.UtcNow
            };

            await SendAsync(ApiResponse<GetEmailStatisticsResponse>.SuccessResult(response, "Email statistics retrieved successfully"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get email statistics");
            await SendAsync(ApiResponse<GetEmailStatisticsResponse>.ErrorResult("Failed to retrieve email statistics"), cancellation: ct);
        }
    }
} 