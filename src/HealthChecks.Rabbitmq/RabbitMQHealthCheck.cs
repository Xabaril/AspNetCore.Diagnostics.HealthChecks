using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ
{
    public class RabbitMQHealthCheck : IHealthCheck, IDisposable
    {
        private IConnection? _connection;
        private IConnectionFactory? _factory;
        private readonly Uri? _rabbitConnectionString;
        private readonly SslOption? _sslOption;
        private readonly bool _ownsConnection;
        private bool _disposed;

        public RabbitMQHealthCheck(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public RabbitMQHealthCheck(IConnectionFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _ownsConnection = true;
        }

        public RabbitMQHealthCheck(Uri rabbitConnectionString, SslOption? ssl)
        {
            _rabbitConnectionString = rabbitConnectionString;
            _sslOption = ssl;
            _ownsConnection = true;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var model = EnsureConnection().CreateModel();
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }

        private IConnection EnsureConnection()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMQHealthCheck));

            if (_connection == null)
            {
                if (_factory == null)
                {
                    _factory = new ConnectionFactory()
                    {
                        Uri = _rabbitConnectionString,
                        AutomaticRecoveryEnabled = true,
                        UseBackgroundThreadsForIO = true,
                    };

                    if (_sslOption != null)
                        ((ConnectionFactory)_factory).Ssl = _sslOption;
                }

                _connection = _factory.CreateConnection();
            }

            return _connection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose connection only if RabbitMQHealthCheck owns it
                if (!_disposed && _connection != null && _ownsConnection)
                {
                    _connection.Dispose();
                    _connection = null;
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
