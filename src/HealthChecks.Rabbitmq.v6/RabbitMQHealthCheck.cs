using System.Collections.Concurrent;
using System.Collections.ObjectModel;
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
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                { "health_check.name", nameof(RabbitMQHealthCheck) },
                { "health_check.task", "ready" },
                { "messaging.system", "rabbitmq" }
    };

    public RabbitMQHealthCheck(RabbitMQHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);
        _connection = options.Connection;

        if (_connection is null && _options.ConnectionFactory is null && _options.ConnectionUri is null)
        {
            throw new ArgumentException("A connection, connection factory, or connection string must be set!", nameof(options));
        }
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        // TODO: cancellationToken unused, see https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/714
        try
        {

            checkDetails.Add("server.address", _connection!.Endpoint.HostName);
            checkDetails.Add("server.port", _connection.Endpoint.Port);
            checkDetails.Add("network.protocol.name", _connection.Endpoint.Protocol.ApiName);
            checkDetails.Add("network.protocol.version", $"{_connection.Endpoint.Protocol.MajorVersion}.{_connection.Endpoint.Protocol.MinorVersion}.{_connection.Endpoint.Protocol.Revision}");
            checkDetails.Add("network.local.port", _connection.LocalPort);
            checkDetails.Add("network.remote.port", _connection.RemotePort);
            using var model = EnsureConnection().CreateModel();
            return Task.FromResult(HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails)));
        }
    }

    private IConnection EnsureConnection()
    {
        _connection ??= _connections.GetOrAdd(_options, _ =>
        {
            var factory = _options.ConnectionFactory;

            if (factory is null)
            {
                Guard.ThrowIfNull(_options.ConnectionUri);
                factory = new ConnectionFactory
                {
                    Uri = _options.ConnectionUri,
                    AutomaticRecoveryEnabled = true
                };

                if (_options.RequestedConnectionTimeout is not null)
                {
                    ((ConnectionFactory)factory).RequestedConnectionTimeout = _options.RequestedConnectionTimeout.Value;
                }

                if (_options.Ssl is not null)
                {
                    ((ConnectionFactory)factory).Ssl = _options.Ssl;
                }
            }

            return factory.CreateConnection();
        });

        return _connection;
    }
}
