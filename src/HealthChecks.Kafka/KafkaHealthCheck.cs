using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kafka
{
    public class KafkaHealthCheck : IHealthCheck
    {
        private readonly ProducerConfig _configuration;
        public KafkaHealthCheck(ProducerConfig configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var producer = new ProducerBuilder<string, string>(_configuration).Build())
                {
                    var message = new Message<string, string>()
                    {
                        Key = "healtcheck-key",
                        Value = $"Check Kafka healthy on {DateTime.UtcNow}"
                    };

                    var result = await producer.ProduceAsync("healthchecks-topic", message);

                    if (result.Status == PersistenceStatus.NotPersisted)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Message is not persisted or a failure is raised on health check for kafka.");
                    }

                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
