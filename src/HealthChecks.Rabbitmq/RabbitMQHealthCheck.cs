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
    private readonly IConnectionFactory? _factory;
    private readonly RabbitMQHealthCheckOptions _options;

    public RabbitMQHealthCheck(IConnection connection)
    {
        _connection = Guard.ThrowIfNull(connection);
        _options = new RabbitMQHealthCheckOptions(new Uri(connection.Endpoint.ToString()));
    }

    public RabbitMQHealthCheck(IConnectionFactory factory)
    {
        _factory = Guard.ThrowIfNull(factory);
        _options = new RabbitMQHealthCheckOptions(factory.Uri);
    }

    public RabbitMQHealthCheck(Uri rabbitConnectionString, SslOption? ssl)
    {
        _factory = new ConnectionFactory
        {
            Uri = rabbitConnectionString,
            AutomaticRecoveryEnabled = true,
            UseBackgroundThreadsForIO = true,
        };
        _options = new RabbitMQHealthCheckOptions(rabbitConnectionString);

        if (ssl != null)
        {
            ((ConnectionFactory)_factory).Ssl = ssl;
            _options.Ssl = ssl;
        }
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
            _connection = _connections.GetOrAdd(_options, _ => _factory.CreateConnection());
        }

        return _connection;
    }
}
