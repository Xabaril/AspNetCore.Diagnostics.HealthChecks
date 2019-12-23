using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kafka
{
    public class KafkaHealthCheck : IHealthCheck
    {
        private readonly IProducer<string, string> _producer;

        public KafkaHealthCheck(ProducerConfig configuration)
        {
            if (configuration == null) 
                throw new ArgumentNullException(nameof(configuration));
            
            _producer = new ProducerBuilder<string, string>(configuration).Build();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>()
                    {
                        Key = "healthcheck-key",
                        Value = $"Check Kafka healthy on {DateTime.UtcNow}"
                    };

                    var result = await _producer.ProduceAsync("healthchecks-topic", message);

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
    }
}
