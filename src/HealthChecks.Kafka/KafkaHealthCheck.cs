using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public KafkaHealthCheck(Dictionary<string, object> configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var producer = new Producer<string, string>(_configuration, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
                {
                    var result = await producer.ProduceAsync("beatpulse-topic", "beatpulse-key", $"Check Kafka healthy on {DateTime.UtcNow}");

                    if (result.Error.Code != ErrorCode.NoError)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"ErrorCode {result.Error.Code} with reason ('{result.Error.Reason}')");
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
