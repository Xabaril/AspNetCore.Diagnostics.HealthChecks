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
        private IConnection _connection;

        public RabbitMQHealthCheck(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (_connection.CreateModel())
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
