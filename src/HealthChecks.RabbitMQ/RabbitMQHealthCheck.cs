using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.RabbitMQ
{
    public class RabbitMQHealthCheck 
        : IHealthCheck
    {
        private readonly string _rabbitMqConnectionString;
        private readonly ILogger<RabbitMQHealthCheck> _logger;

        public RabbitMQHealthCheck(string rabbitMqConnectionString, ILogger<RabbitMQHealthCheck> logger = null)
        {
            _rabbitMqConnectionString = rabbitMqConnectionString ?? throw new ArgumentNullException(nameof(rabbitMqConnectionString));
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(RabbitMQHealthCheck)} is checking the RabbitMQ host.");

                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(_rabbitMqConnectionString)
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {

                    _logger?.LogInformation($"The {nameof(RabbitMQHealthCheck)} check success.");

                    return Task.FromResult(
                        HealthCheckResult.Passed());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(RabbitMQHealthCheck)} check fail with the exception {ex.ToString()}.");

                return Task.FromResult(
                        HealthCheckResult.Failed(exception: ex));
            }
        }
    }
}
