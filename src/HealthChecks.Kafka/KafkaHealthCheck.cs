using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kafka
{
    /// <summary>
    /// A health check for Kafka cluster.
    /// </summary>
    public class KafkaHealthCheck : IHealthCheck, IDisposable
    {
        private readonly KafkaHealthCheckOptions _options;
        private IProducer<string, string>? _producer;

        public KafkaHealthCheck(KafkaHealthCheckOptions options)
        {
            Guard.ThrowIfNull(options.Configuration);
            _options = options;
        }

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

                var result = await _producer.ProduceAsync(_options.Topic ?? KafkaHealthCheckBuilderExtensions.DEFAULT_TOPIC, message, cancellationToken).ConfigureAwait(false);

                if (result.Status == PersistenceStatus.NotPersisted)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Message is not persisted or a failure is raised on health check for kafka.");
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        public void Dispose() => _producer?.Dispose();
    }
}
