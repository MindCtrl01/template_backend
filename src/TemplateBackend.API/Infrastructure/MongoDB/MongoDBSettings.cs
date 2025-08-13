namespace TemplateBackend.API.Infrastructure.MongoDB;

/// <summary>
/// MongoDB settings configuration
/// </summary>
public class MongoDBSettings
{
    /// <summary>
    /// MongoDB database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Email collection name
    /// </summary>
    public string EmailCollection { get; set; } = string.Empty;

    /// <summary>
    /// Log collection name
    /// </summary>
    public string LogCollection { get; set; } = string.Empty;
} 