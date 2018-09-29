using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Kafka
{
    public class KafkaHealthCheck : IHealthCheck
    {
        private readonly Dictionary<string, object> _configuration;
        private readonly ILogger<KafkaHealthCheck> _logger;

        public KafkaHealthCheck(Dictionary<string, object> configuration, ILogger<KafkaHealthCheck> logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(KafkaHealthCheck)} is checking the Kafka broker.");

                using (var producer = new Producer<Null, string>(_configuration, null, new StringSerializer(Encoding.UTF8)))
                {
                    var result = await producer.ProduceAsync("beatpulse-topic", null, $"Check Kafka healthy on {DateTime.UtcNow}");

                    if (result.Error.Code != ErrorCode.NoError)
                    {
                        _logger?.LogWarning($"The {nameof(KafkaHealthCheck)} check failed.");

                        return HealthCheckResult.Failed($"ErrorCode {result.Error.Code} with reason ('{result.Error.Reason}')");
                    }

                    _logger?.LogInformation($"The {nameof(KafkaHealthCheck)} check success.");

                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(KafkaHealthCheck)} check fail for Kafka broker with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
