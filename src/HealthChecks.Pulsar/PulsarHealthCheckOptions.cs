using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.Pulsar;

/// <summary>
/// Options for <see cref="PulsarHealthCheck"/>.
/// </summary>
public class PulsarHealthCheckOptions
{
    /// <summary>
    /// The topic name to produce Pulsar messages on.
    /// </summary>
    public string Topic { get; set; } = PulsarHealthCheckBuilderExtensions.DEFAULT_TOPIC;

    /// <summary>
    /// Optional delegate to configure Pulsar producer.
    /// </summary>
    public Action<IProducerBuilder<string>>? Configure { get; set; }

    /// <summary>
    /// Delegate to build a message being send to Pulsar.
    /// </summary>
    public Func<PulsarHealthCheckOptions, string> MessageBuilder { get; set; } = _ => $"Check Pulsar healthy on {DateTime.UtcNow}";

    /// <summary>
    /// Delegate to build a message being send to Pulsar.
    /// </summary>
    public Func<PulsarHealthCheckOptions, IMessageBuilder<string>, CancellationToken, ValueTask<MessageId>> MessageSender { get; set; } = (o, builder, ct) => builder
        .Key("healthcheck-key")
        .Send(o.MessageBuilder(o), ct);
}
