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

        private Uri _rabbitConnectionString;
        private SslOption _sslOption;

        public RabbitMQHealthCheck(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public RabbitMQHealthCheck(Uri rabbitConnectionString, SslOption ssl)
        {
            _rabbitConnectionString = rabbitConnectionString;
            _sslOption = ssl;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureConnection();

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

        private void EnsureConnection()
        {
            if(_connection == null )
            {
                var connectionFactory = new ConnectionFactory()
                {
                    Uri = _rabbitConnectionString,
                    AutomaticRecoveryEnabled = true,
                    UseBackgroundThreadsForIO = true
                };

                if (_sslOption != null)
                {
                    connectionFactory.Ssl = _sslOption;
                }

                _connection = connectionFactory.CreateConnection();
            }
        }
    }
}
