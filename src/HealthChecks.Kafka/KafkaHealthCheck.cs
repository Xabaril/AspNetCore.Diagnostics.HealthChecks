using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kafka
{
    public class KafkaHealthCheck : IHealthCheck, IDisposable
    {
        private readonly ProducerConfig _configuration;
        private readonly string _topic;
        private IProducer<string, string>? _producer;

        public KafkaHealthCheck(ProducerConfig configuration, string? topic)
        {
            _configuration = Guard.ThrowIfNull(configuration);
            _topic = topic ?? "healthchecks-topic";
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _producer ??= new ProducerBuilder<string, string>(_configuration).Build();

                var message = new Message<string, string>
                {
                    Key = "healthcheck-key",
                    Value = $"Check Kafka healthy on {DateTime.UtcNow}"
                };

                var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

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
