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
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _rmqConnection;

        public RabbitMQHealthCheck(string rabbitMqConnectionString, SslOption sslOption = null)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqConnectionString ?? throw new ArgumentNullException(nameof(rabbitMqConnectionString))),
                AutomaticRecoveryEnabled = true // Explicitly setting to ensure this is true (in case the default changes)
            };

            if (sslOption != null)
            {
                connectionFactory.Ssl = sslOption;
            }

            _connectionFactory = connectionFactory;
        }

        public RabbitMQHealthCheck(IConnection connection)
        {
            _rmqConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public RabbitMQHealthCheck(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // If no factory was provided then we're stuck using the passed in connection
                // regardless of the state it may be in. We don't have a way to attempt to
                // create a new connection :(
                if (_connectionFactory == null)
                {
                    return TestConnection(_rmqConnection);
                }

                using (var connection = _connectionFactory.CreateConnection())
                {
                    return TestConnection(connection);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }

        private static Task<HealthCheckResult> TestConnection(IConnection connection)
        {
            using (connection.CreateModel())
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy());
            }
        }

        private static IConnection CreateConnection(IConnectionFactory connectionFactory)
        {
            return connectionFactory.CreateConnection("Health Check Connection");
        }
    }
}
