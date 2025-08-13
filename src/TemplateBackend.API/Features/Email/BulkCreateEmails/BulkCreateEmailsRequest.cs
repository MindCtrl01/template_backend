using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Email.BulkCreateEmails;

/// <summary>
/// Request model for bulk email creation
/// </summary>
public class BulkCreateEmailsRequest
{
    /// <summary>
    /// List of emails to create
    /// </summary>
    [Required]
    public List<EmailDto> Emails { get; set; } = new();
}

/// <summary>
/// Email data transfer object
/// </summary>
public class EmailDto
{
    /// <summary>
    /// Recipient email address
    /// </summary>
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Email subject
    /// </summary>
    [Required]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content
    /// </summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Type of email (e.g., "verification", "notification", "marketing")
    /// </summary>
    public string Type { get; set; } = "general";

    /// <summary>
    /// Associated user ID (optional)
    /// </summary>
    public int? UserId { get; set; }
}