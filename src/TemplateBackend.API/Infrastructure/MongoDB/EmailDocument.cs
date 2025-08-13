using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TemplateBackend.API.Infrastructure.MongoDB;

/// <summary>
/// Email document for MongoDB storage
/// </summary>
public class EmailDocument
{
    /// <summary>
    /// MongoDB ObjectId
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email recipient
    /// </summary>
    [BsonElement("to")]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Email subject
    /// </summary>
    [BsonElement("subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body (HTML)
    /// </summary>
    [BsonElement("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Email type (LoginNotification, RegistrationConfirmation, PasswordReset, etc.)
    /// </summary>
    [BsonElement("emailType")]
    public string EmailType { get; set; } = string.Empty;

    /// <summary>
    /// User ID associated with the email (if applicable)
    /// </summary>
    [BsonElement("userId")]
    public int? UserId { get; set; }

    /// <summary>
    /// User email associated with the email
    /// </summary>
    [BsonElement("userEmail")]
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// User name associated with the email
    /// </summary>
    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata (IP address, location, etc.)
    /// </summary>
    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Email status (Sent, Failed, Pending)
    /// </summary>
    [BsonElement("status")]
    public string Status { get; set; } = "Sent";

    /// <summary>
    /// Error message if email failed to send
    /// </summary>
    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    [BsonElement("retryCount")]
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Date and time when the email was sent
    /// </summary>
    [BsonElement("sentAt")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the email was created
    /// </summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the email was last updated
    /// </summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 