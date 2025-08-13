using FastEndpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TemplateBackend.API.Features.Health;

/// <summary>
/// FastEndpoint for health checks
/// </summary>
public class HealthEndpoint : EndpointWithoutRequest<object>
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthEndpoint> _logger;

    public HealthEndpoint(HealthCheckService healthCheckService, ILogger<HealthEndpoint> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/health");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Health check";
            s.Description = "Returns the health status of the application and its dependencies";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();

            var healthInfo = new
            {
                Status = report.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Duration = report.TotalDuration,
                Entries = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Duration = entry.Value.Duration,
                    Tags = entry.Value.Tags
                }).ToList()
            };

            await SendAsync(healthInfo, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            await SendAsync(new { Status = "Unhealthy", Error = "Health check failed" }, cancellation: ct);
        }
    }
} 