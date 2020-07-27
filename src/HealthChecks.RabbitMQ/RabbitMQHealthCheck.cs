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
        
        private IConnectionFactory _factory;
        private readonly Uri _rabbitConnectionString;
        private readonly SslOption _sslOption;

        public RabbitMQHealthCheck(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
        public RabbitMQHealthCheck(IConnectionFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
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
                if (_factory == null)
                {
                    _factory = new ConnectionFactory()
                    {
                        Uri = _rabbitConnectionString,
                        AutomaticRecoveryEnabled = true,
                        UseBackgroundThreadsForIO = true,
                        Ssl = _sslOption ?? new SslOption()
                    };
                }

                _connection = _factory.CreateConnection();
            }
        }
    }
}
