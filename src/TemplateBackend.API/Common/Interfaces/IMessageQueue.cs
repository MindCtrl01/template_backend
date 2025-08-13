namespace TemplateBackend.API.Common.Interfaces;

/// <summary>
/// Abstract message queue interface for pub-sub pattern
/// </summary>
public interface IMessageQueue
{
    /// <summary>
    /// Publishes a message to a topic
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topic">Topic name</param>
    /// <param name="message">Message to publish</param>
    /// <param name="key">Message key (optional)</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishAsync<T>(string topic, T message, string? key = null) where T : class;

    /// <summary>
    /// Subscribes to a topic and processes messages
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topic">Topic name</param>
    /// <param name="groupId">Consumer group ID</param>
    /// <param name="handler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SubscribeAsync<T>(string topic, string groupId, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Subscribes to multiple topics and processes messages
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <param name="topics">List of topic names</param>
    /// <param name="groupId">Consumer group ID</param>
    /// <param name="handler">Message handler function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SubscribeAsync<T>(IEnumerable<string> topics, string groupId, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the current status of the message queue
    /// </summary>
    /// <returns>Queue status</returns>
    Task<QueueStatus> GetStatusAsync();

    /// <summary>
    /// Disposes the message queue
    /// </summary>
    void Dispose();
}

/// <summary>
/// Queue status information
/// </summary>
public class QueueStatus
{
    /// <summary>
    /// Whether the queue is connected
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Connection details
    /// </summary>
    public string ConnectionDetails { get; set; } = string.Empty;

    /// <summary>
    /// Number of active consumers
    /// </summary>
    public int ActiveConsumers { get; set; }

    /// <summary>
    /// Number of active producers
    /// </summary>
    public int ActiveProducers { get; set; }

    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Timestamp of last activity
    /// </summary>
    public DateTime LastActivity { get; set; }
} 