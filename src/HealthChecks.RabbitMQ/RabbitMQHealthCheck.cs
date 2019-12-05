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

            _lazyConnectionFactory = new Lazy<IConnectionFactory>(() => new ConnectionFactory()
            {
                Uri = new Uri(rabbitMqConnectionString),
                Ssl = sslOption ?? new SslOption()
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
