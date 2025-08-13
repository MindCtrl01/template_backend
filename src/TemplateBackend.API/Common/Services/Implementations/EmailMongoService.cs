using MongoDB.Driver;
using MongoDB.Bson;
using TemplateBackend.API.Infrastructure.MongoDB;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Email MongoDB service implementation
/// </summary>
public class EmailMongoService : IEmailMongoService
{
    private readonly IMongoDBService<EmailDocument> _mongoDBService;
    private readonly ILogger<EmailMongoService> _logger;

    public EmailMongoService(IMongoDBService<EmailDocument> mongoDBService, ILogger<EmailMongoService> logger)
    {
        _mongoDBService = mongoDBService;
        _logger = logger;
    }

    public async Task<EmailDocument> SaveEmailAsync(EmailDocument emailDocument)
    {
        try
        {
            emailDocument.CreatedAt = DateTime.UtcNow;
            emailDocument.UpdatedAt = DateTime.UtcNow;
            emailDocument.SentAt = DateTime.UtcNow;

            var savedDocument = await _mongoDBService.CreateAsync(emailDocument);
            _logger.LogInformation("Email saved to MongoDB: {EmailId}", savedDocument.Id);
            return savedDocument;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save email to MongoDB");
            throw;
        }
    }

    public async Task<List<EmailDocument>> GetEmailsByUserIdAsync(int userId, int limit = 50)
    {
        try
        {
            var emails = await _mongoDBService.FindAsync(
                filter: e => e.UserId == userId,
                sort: e => e.CreatedAt,
                limit: limit
            );
            
            return emails.OrderByDescending(e => e.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get emails by user ID: {UserId}", userId);
            return new List<EmailDocument>();
        }
    }

    public async Task<List<EmailDocument>> GetEmailsByTypeAsync(string emailType, int limit = 50)
    {
        try
        {
            var emails = await _mongoDBService.FindAsync(
                filter: e => e.EmailType == emailType,
                sort: e => e.CreatedAt,
                limit: limit
            );
            
            return emails.OrderByDescending(e => e.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get emails by type: {EmailType}", emailType);
            return new List<EmailDocument>();
        }
    }

    public async Task<List<EmailDocument>> GetEmailsByStatusAsync(string status, int limit = 50)
    {
        try
        {
            var emails = await _mongoDBService.FindAsync(
                filter: e => e.Status == status,
                sort: e => e.CreatedAt,
                limit: limit
            );
            
            return emails.OrderByDescending(e => e.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get emails by status: {Status}", status);
            return new List<EmailDocument>();
        }
    }

    public async Task<EmailDocument?> UpdateEmailStatusAsync(string emailId, string status, string? errorMessage = null)
    {
        try
        {
            var existingEmail = await _mongoDBService.GetByIdAsync(emailId);
            if (existingEmail == null)
            {
                _logger.LogWarning("Email not found for status update: {EmailId}", emailId);
                return null;
            }

            existingEmail.Status = status;
            existingEmail.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                existingEmail.ErrorMessage = errorMessage;
            }

            if (status == "Failed")
            {
                existingEmail.RetryCount++;
            }

            var updatedEmail = await _mongoDBService.UpdateAsync(emailId, existingEmail);
            _logger.LogInformation("Email status updated: {EmailId} -> {Status}", emailId, status);
            return updatedEmail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update email status: {EmailId}", emailId);
            return null;
        }
    }

    public async Task<EmailStatistics> GetEmailStatisticsAsync()
    {
        try
        {
            var statistics = new EmailStatistics();

            // Get total emails
            statistics.TotalEmails = (int)await _mongoDBService.CountAsync();

            // Get emails by status
            statistics.SentEmails = (int)await _mongoDBService.CountAsync(e => e.Status == "Sent");
            statistics.FailedEmails = (int)await _mongoDBService.CountAsync(e => e.Status == "Failed");
            statistics.PendingEmails = (int)await _mongoDBService.CountAsync(e => e.Status == "Pending");

            // Get emails by type
            var allEmails = await _mongoDBService.FindAsync();
            var emailsByType = allEmails.GroupBy(e => e.EmailType)
                .ToDictionary(g => g.Key, g => g.Count());
            statistics.EmailsByType = emailsByType;

            // Get emails by date (last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentEmails = await _mongoDBService.FindAsync(e => e.CreatedAt >= thirtyDaysAgo);
            var emailsByDate = recentEmails.GroupBy(e => e.CreatedAt.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Count());
            statistics.EmailsByDate = emailsByDate;

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get email statistics");
            return new EmailStatistics();
        }
    }

    public async Task<List<EmailDocument>> GetAllEmailsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var emails = await _mongoDBService.GetAllAsync(page, pageSize);
            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all emails");
            return new List<EmailDocument>();
        }
    }

    public async Task<EmailDocument?> GetEmailByIdAsync(string emailId)
    {
        try
        {
            return await _mongoDBService.GetByIdAsync(emailId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get email by ID: {EmailId}", emailId);
            return null;
        }
    }

    public async Task<bool> DeleteEmailAsync(string emailId)
    {
        try
        {
            return await _mongoDBService.DeleteAsync(emailId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete email: {EmailId}", emailId);
            return false;
        }
    }
} 