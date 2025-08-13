using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Features.Email.BulkCreateEmails;

/// <summary>
/// Response model for bulk email creation
/// </summary>
public class BulkCreateEmailsResponse
{
    /// <summary>
    /// Number of emails created
    /// </summary>
    public int CreatedCount { get; set; }

    /// <summary>
    /// List of created email documents
    /// </summary>
    public List<EmailDocument> CreatedEmails { get; set; } = new();

    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}