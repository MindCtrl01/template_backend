using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Features.Email.BulkCreateEmails;

/// <summary>
/// FastEndpoint for bulk email creation
/// </summary>
public class BulkCreateEmailsEndpoint : Endpoint<BulkCreateEmailsRequest, ApiResponse<BulkCreateEmailsResponse>>
{
    private readonly IMongoDBService<EmailDocument> _mongoDBService;
    private readonly ILogger<BulkCreateEmailsEndpoint> _logger;

    public BulkCreateEmailsEndpoint(
        IMongoDBService<EmailDocument> mongoDBService,
        ILogger<BulkCreateEmailsEndpoint> logger)
    {
        _mongoDBService = mongoDBService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/email/bulk-create");
        Summary(s =>
        {
            s.Summary = "Bulk create emails";
            s.Description = "Creates multiple email documents in bulk using MongoDB";
        });
    }

    public override async Task HandleAsync(BulkCreateEmailsRequest req, CancellationToken ct)
    {
        try
        {
            // Convert DTOs to EmailDocument objects
            var emailDocuments = req.Emails.Select(email => new EmailDocument
            {
                To = email.To,
                Subject = email.Subject,
                Body = email.Body,
                EmailType = email.Type,
                UserId = email.UserId,
                Status = "Pending",
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            // Perform bulk insert
            var createdEmails = await _mongoDBService.CreateManyAsync(emailDocuments);

            var response = new BulkCreateEmailsResponse
            {
                CreatedCount = createdEmails.Count,
                CreatedEmails = createdEmails,
                Message = $"Successfully created {createdEmails.Count} email documents"
            };

            _logger.LogInformation("Bulk email creation completed. Created {Count} emails", createdEmails.Count);

            await SendAsync(ApiResponse<BulkCreateEmailsResponse>.SuccessResult(response, "Bulk email creation completed successfully"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform bulk email creation");
            await SendAsync(ApiResponse<BulkCreateEmailsResponse>.ErrorResult("Failed to create emails in bulk"), cancellation: ct);
        }
    }
} 