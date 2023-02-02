using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// A health check for RabbitMQ services.
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<RabbitMQHealthCheckOptions, IConnection> _connections = new();

    private IConnection? _connection;
    private readonly RabbitMQHealthCheckOptions _options;

    public RabbitMQHealthCheck(RabbitMQHealthCheckOptions options)
    {
        _options = options;
        _connection = options.Connection;

        if (_connection is null && _options.ConnectionFactory is null && _options.ConnectionUri is null)
        {
            throw new ArgumentException("A connection, connnection factory, or connection string must be set!", nameof(options));
        }
    }

    [Obsolete("Please provide RabbitMQHealthCheckOptions")]
    public RabbitMQHealthCheck(IConnection connection) : this(new RabbitMQHealthCheckOptions { Connection = connection })
    { }

    [Obsolete("Please provide RabbitMQHealthCheckOptions")]
    public RabbitMQHealthCheck(IConnectionFactory factory) : this(new RabbitMQHealthCheckOptions { ConnectionFactory = factory })
    { }

    [Obsolete("Please provide RabbitMQHealthCheckOptions")]
    public RabbitMQHealthCheck(Uri rabbitConnectionString, SslOption? ssl) : this(new RabbitMQHealthCheckOptions { ConnectionUri = rabbitConnectionString, Ssl = ssl })
    { }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var model = EnsureConnection().CreateModel();
            return HealthCheckResultTask.Healthy;
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }

    private IConnection EnsureConnection()
    {
        if (_connection is null)
        {
            _connection = _connections.GetOrAdd(_options, _ =>
            {
                var factory = _options.ConnectionFactory;

                if (factory is null)
                {
                    Guard.ThrowIfNull(_options.ConnectionUri);
                    factory = new ConnectionFactory
                    {
                        Uri = _options.ConnectionUri,
                        AutomaticRecoveryEnabled = true,
                        UseBackgroundThreadsForIO = true,
                    };

                    if (_options.Ssl is not null)
                    {
                        ((ConnectionFactory)factory).Ssl = _options.Ssl;
                    }
                }

                return factory.CreateConnection();
            });
        }

        return _connection;
    }
}
