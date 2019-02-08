using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public RabbitMQHealthCheck(string rabbitMqConnectionString)
        {
            _rabbitMqConnectionString = rabbitMqConnectionString ?? throw new ArgumentNullException(nameof(rabbitMqConnectionString));
        }
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(_rabbitMqConnectionString)
                };

                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    return Task.FromResult(
                        HealthCheckResult.Healthy());
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
