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
        private readonly Lazy<IConnectionFactory> _lazyConnectionFactory;
        private readonly IConnection _rmqConnection;

        public RabbitMQHealthCheck(string rabbitMqConnectionString, SslOption sslOption = null)
        {
            if (rabbitMqConnectionString == null) throw new ArgumentNullException(nameof(rabbitMqConnectionString));

            
            _lazyConnectionFactory = new Lazy<IConnectionFactory>(() =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    Uri = new Uri(rabbitMqConnectionString),
                    AutomaticRecoveryEnabled = true // Explicitly setting to ensure this is true (in case the default changes)
                };

                if (sslOption != null)
                {
                    connectionFactory.Ssl = sslOption;
                }

                return connectionFactory;
            });
        }

        public RabbitMQHealthCheck(IConnection connection)
        {
            _rmqConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public RabbitMQHealthCheck(IConnectionFactory connectionFactory)
        {
            _lazyConnectionFactory = new Lazy<IConnectionFactory>(() => 
                connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory)));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_rmqConnection != null)
                {
                    return TestConnection(_rmqConnection);
                }

                using (var connection = CreateConnection(_lazyConnectionFactory.Value))
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
            using (var channel = connection.CreateModel())
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
