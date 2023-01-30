using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// A health check for RabbitMQ services.
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, IConnection> _connections = new();

    private IConnection? _connection;
    private readonly IConnectionFactory? _factory;

    public RabbitMQHealthCheck(IConnection connection)
    {
        _connection = Guard.ThrowIfNull(connection);
    }

    public RabbitMQHealthCheck(IConnectionFactory factory)
    {
        _factory = Guard.ThrowIfNull(factory);
    }

    public RabbitMQHealthCheck(Uri rabbitConnectionString, SslOption? ssl)
    {
        _factory = new ConnectionFactory
        {
            Uri = rabbitConnectionString,
            AutomaticRecoveryEnabled = true,
            UseBackgroundThreadsForIO = true,
        };

        if (ssl!= null)
            ((ConnectionFactory)_factory).Ssl = ssl;
    }

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
        if (_connection == null)
        {
            Guard.ThrowIfNull(_factory);
            _connection = _connections.GetOrAdd(_factory.Uri.ToString(), _ => _factory.CreateConnection());
        }

        return _connection;
    }
}
