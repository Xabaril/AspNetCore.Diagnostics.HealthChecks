using System.Collections.ObjectModel;
using System.Net;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kafka;

/// <summary>
/// A health check for Kafka cluster.
/// </summary>
public class KafkaHealthCheck : IHealthCheck, IDisposable
{
    private readonly KafkaHealthCheckOptions _options;
    private IProducer<string, string>? _producer;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                { "health_check.name", nameof(KafkaHealthCheck) },
                { "health_check.task", "ready" },
                { "messaging.system", "kafka" }
    };

    public KafkaHealthCheck(KafkaHealthCheckOptions options)
    {
        Guard.ThrowIfNull(options.Configuration);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_producer == null)
            {
                var builder = new ProducerBuilder<string, string>(_options.Configuration);
                _options.Configure?.Invoke(builder);
                _producer ??= builder.Build();
            }

            var message = _options.MessageBuilder(_options);
            var topic = _options.Topic ?? KafkaHealthCheckBuilderExtensions.DEFAULT_TOPIC;

            checkDetails.Add("messaging.destination.name", topic);

            var result = await _producer.ProduceAsync(topic, message, cancellationToken).ConfigureAwait(false);

            if (result.Status == PersistenceStatus.NotPersisted)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Message is not persisted or a failure is raised on health check for kafka.", data: new ReadOnlyDictionary<string, object>(checkDetails));
            }

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }

    public virtual void Dispose() => _producer?.Dispose();
}
