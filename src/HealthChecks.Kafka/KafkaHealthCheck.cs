using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Kafka
{
    public class KafkaHealthCheck : IHealthCheck
    {
        private readonly ProducerConfig _configuration;
        private readonly string _topic;
        private readonly Action<LogMessage> _kafkaLogHandler;

        private IProducer<string, string> _producer;

        public KafkaHealthCheck(ProducerConfig configuration, string topic, Action<LogMessage> kafkaLogHandler)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _kafkaLogHandler = kafkaLogHandler;
            _topic = topic ?? "healthchecks-topic";
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_producer == null)
                {
                    var producerBuilder = new ProducerBuilder<string, string>(_configuration);
                    if (_kafkaLogHandler != null)
                    {
                        producerBuilder.SetLogHandler((_, logMessage) => _kafkaLogHandler(logMessage));
                    }
                   
                    _producer = producerBuilder.Build();
                }

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
    }
}
