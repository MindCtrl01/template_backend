namespace TemplateBackend.API.Features.Payments.CreateSubscription;

/// <summary>
/// Create subscription response
/// </summary>
public class CreateSubscriptionResponse
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Subscription ID
    /// </summary>
    public string SubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// Subscription status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error message if any
    /// </summary>
    public string? ErrorMessage { get; set; }
} 