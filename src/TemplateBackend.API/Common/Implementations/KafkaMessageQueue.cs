using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TemplateBackend.API.Common.Interfaces;
using System.Text.Json;

namespace TemplateBackend.API.Common.Implementations;

/// <summary>
/// Kafka message queue implementation
/// </summary>
public class KafkaMessageQueue : IMessageQueue, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaMessageQueue> _logger;
    private bool _disposed = false;

    public KafkaMessageQueue(IOptions<KafkaSettings> settings, ILogger<KafkaMessageQueue> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Configure producer
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            ClientId = _settings.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            LingerMs = 5,
            BatchSize = 16384,
            CompressionType = CompressionType.Snappy
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetLogHandler((_, message) => _logger.LogInformation("Kafka Producer: {Message}", message.Message))
            .SetErrorHandler((_, error) => _logger.LogError("Kafka Producer Error: {Error}", error.Reason))
            .Build();

        // Configure consumer
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true,
            SessionTimeoutMs = 30000,
            HeartbeatIntervalMs = 10000,
            MaxPollIntervalMs = 300000
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetLogHandler((_, message) => _logger.LogInformation("Kafka Consumer: {Message}", message.Message))
            .SetErrorHandler((_, error) => _logger.LogError("Kafka Consumer Error: {Error}", error.Reason))
            .Build();
    }

    public async Task PublishAsync<T>(string topic, T message, string? key = null) where T : class
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = key ?? Guid.NewGuid().ToString(),
                Value = jsonMessage
            };

            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage);
            
            _logger.LogInformation("Message published to topic {Topic}, partition {Partition}, offset {Offset}", 
                deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to topic {Topic}", topic);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string topic, string groupId, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        await SubscribeAsync<T>(new[] { topic }, groupId, handler, cancellationToken);
    }

    public async Task SubscribeAsync<T>(IEnumerable<string> topics, string groupId, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            _consumer.Subscribe(topics);

            _logger.LogInformation("Subscribed to topics: {Topics} with group ID: {GroupId}", 
                string.Join(", ", topics), groupId);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (consumeResult.IsPartitionEOF)
                    {
                        _logger.LogDebug("Reached end of partition {Partition}", consumeResult.Partition);
                        continue;
                    }

                    _logger.LogDebug("Received message from topic {Topic}, partition {Partition}, offset {Offset}", 
                        consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

                    try
                    {
                        var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                        if (message != null)
                        {
                            await handler(message);
                        }

                        // Commit the offset after successful processing
                        _consumer.Commit(consumeResult);
                        
                        _logger.LogDebug("Successfully processed message from topic {Topic}, offset {Offset}", 
                            consumeResult.Topic, consumeResult.Offset);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize message from topic {Topic}", consumeResult.Topic);
                        // Commit the offset to skip the malformed message
                        _consumer.Commit(consumeResult);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process message from topic {Topic}, offset {Offset}", 
                            consumeResult.Topic, consumeResult.Offset);
                        // Don't commit the offset to allow retry
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumption cancelled");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topics: {Topics}", string.Join(", ", topics));
            throw;
        }
        finally
        {
            _consumer.Close();
        }
    }

    public async Task<QueueStatus> GetStatusAsync()
    {
        try
        {
            var status = new QueueStatus
            {
                IsConnected = true,
                ConnectionDetails = $"Kafka: {_settings.BootstrapServers}",
                ActiveConsumers = 1, // Simplified for this implementation
                ActiveProducers = 1, // Simplified for this implementation
                LastActivity = DateTime.UtcNow
            };

            return await Task.FromResult(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue status");
            return new QueueStatus
            {
                IsConnected = false,
                LastError = ex.Message,
                LastActivity = DateTime.UtcNow
            };
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            _consumer?.Close();
            _consumer?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Kafka settings configuration
/// </summary>
public class KafkaSettings
{
    /// <summary>
    /// Kafka bootstrap servers
    /// </summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>
    /// Client ID for Kafka
    /// </summary>
    public string ClientId { get; set; } = "template-backend";

    /// <summary>
    /// Consumer group ID
    /// </summary>
    public string ConsumerGroupId { get; set; } = "template-backend-group";

    /// <summary>
    /// Payment topic name
    /// </summary>
    public string PaymentTopic { get; set; } = "payment-events";

    /// <summary>
    /// Payment result topic name
    /// </summary>
    public string PaymentResultTopic { get; set; } = "payment-results";

    /// <summary>
    /// Subscription topic name
    /// </summary>
    public string SubscriptionTopic { get; set; } = "subscription-events";

    /// <summary>
    /// Retry topic name
    /// </summary>
    public string RetryTopic { get; set; } = "payment-retry";
} 