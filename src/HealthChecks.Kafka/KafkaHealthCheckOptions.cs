using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Kafka;

/// <summary>
/// Options for <see cref="KafkaHealthCheck"/>.
/// </summary>
public class KafkaHealthCheckOptions
{
    /// <summary>
    /// The Kafka connection configuration parameters to be used.
    /// </summary>
    public ProducerConfig Configuration { get; set; } = null!;

    /// <summary>
    /// The topic name to produce Kafka messages on.
    /// </summary>
    public string Topic { get; set; } = KafkaHealthCheckBuilderExtensions.DEFAULT_TOPIC;

    /// <summary>
    /// Optional delegate to configure Kafka producer.
    /// </summary>
    public Action<ProducerBuilder<string, string>>? Configure { get; set; }

    /// <summary>
    /// Delegate to build a message being send to Kafka.
    /// </summary>
    public Func<KafkaHealthCheckOptions, Message<string, string>> MessageBuilder { get; set; } = _ => new Message<string, string>
    {
        Key = "healthcheck-key",
        Value = $"Check Kafka healthy on {DateTime.UtcNow}"
    };

    public bool A { get; set; }
}
