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
        protected bool AttemptConnectionReuse = false;

        public RabbitMQHealthCheck(string rabbitMqConnectionString, SslOption sslOption = null)
        {
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqConnectionString ?? throw new ArgumentNullException(nameof(rabbitMqConnectionString))),
                // Using background threads to ensure that process shutdown isn't waiting for these IO threads to complete.
                // This addresses issue #412 which reports that the process hangs on shutdown if the connection used
                // for health checks is using foreground threads and isn't explicitly disposed.
                UseBackgroundThreadsForIO = true, 

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

                // We're safe holding on to the connection and not disposing it here because
                // the factory is configured to use background threads and won't cause the
                // process shutdown to hang (as would happen with foreground thread that are still running)
                // and we've been told (through AttemptConnectionReuse) to assume that this is a long
                // lived RabbitMQHealthCheck instance (so we'll reuse this instance and be able to
                // benefit from reusing the internally stored connection).
                if (AttemptConnectionReuse)
                {
                    // Clean up a non-healthy connection that won't end up recovering itself
                    if (_rmqConnection != null && !IsAutoReconnect(_connectionFactory) && !_rmqConnection.IsOpen == false)
                    {
                        _rmqConnection?.Abort(0);
                    }

                    _rmqConnection = _rmqConnection ?? CreateConnection(_connectionFactory);
                    return TestConnection(_rmqConnection);
                }
                // If we're in a situation where we don't know that it's safe to
                // reuse connections then we will create a new connection for each
                // health check evaluation.
                //
                // It is not safe if we don't know that this RabbitMQHealthCheck instance 
                // may be transient, where a new one created for each new health check evaluation,
                // since we would then never reuse this instance to reuse the internally stored connection.
                //
                // It is also not safe if the IConnectionFactory.UseBackgroundThreadsForIO is
                // set to false as the foreground threads of an internally stored connection would
                // prevent the process from terminating since there isn't a hook to dispose
                // the connection on shutdown.
                else
                {
                    using (var connection = CreateConnection(_connectionFactory))
                    {
                        return TestConnection(connection);
                    }
                }
                
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }

        private bool IsAutoReconnect(IConnectionFactory connectionFactory)
        {
            return (connectionFactory as ConnectionFactory)?.AutomaticRecoveryEnabled == true;
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
