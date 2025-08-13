using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// Email MongoDB service interface
/// </summary>
public interface IEmailMongoService
{
    /// <summary>
    /// Saves an email document to MongoDB
    /// </summary>
    /// <param name="emailDocument">Email document to save</param>
    /// <returns>Saved email document</returns>
    Task<EmailDocument> SaveEmailAsync(EmailDocument emailDocument);

    /// <summary>
    /// Gets email documents by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Number of documents to return</param>
    /// <returns>List of email documents</returns>
    Task<List<EmailDocument>> GetEmailsByUserIdAsync(int userId, int limit = 50);

    /// <summary>
    /// Gets email documents by email type
    /// </summary>
    /// <param name="emailType">Email type</param>
    /// <param name="limit">Number of documents to return</param>
    /// <returns>List of email documents</returns>
    Task<List<EmailDocument>> GetEmailsByTypeAsync(string emailType, int limit = 50);

    /// <summary>
    /// Gets email documents by status
    /// </summary>
    /// <param name="status">Email status</param>
    /// <param name="limit">Number of documents to return</param>
    /// <returns>List of email documents</returns>
    Task<List<EmailDocument>> GetEmailsByStatusAsync(string status, int limit = 50);

    /// <summary>
    /// Updates email status
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <param name="status">New status</param>
    /// <param name="errorMessage">Error message if failed</param>
    /// <returns>Updated email document</returns>
    Task<EmailDocument?> UpdateEmailStatusAsync(string emailId, string status, string? errorMessage = null);

    /// <summary>
    /// Gets email statistics
    /// </summary>
    /// <returns>Email statistics</returns>
    Task<EmailStatistics> GetEmailStatisticsAsync();

    /// <summary>
    /// Gets all emails with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Tuple of emails and total count</returns>
    Task<List<EmailDocument>> GetAllEmailsAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Gets email by ID
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <returns>Email document or null</returns>
    Task<EmailDocument?> GetEmailByIdAsync(string emailId);

    /// <summary>
    /// Deletes an email document
    /// </summary>
    /// <param name="emailId">Email ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteEmailAsync(string emailId);
}

/// <summary>
/// Email statistics
/// </summary>
public class EmailStatistics
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
    /// Emails by type
    /// </summary>
    public Dictionary<string, int> EmailsByType { get; set; } = new();

    /// <summary>
    /// Emails by date (last 30 days)
    /// </summary>
    public Dictionary<string, int> EmailsByDate { get; set; } = new();
} 