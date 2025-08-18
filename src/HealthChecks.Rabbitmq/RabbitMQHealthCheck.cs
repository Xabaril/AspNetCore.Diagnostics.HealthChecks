using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// A health check for RabbitMQ services.
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IConnection? _connection;
    private readonly IServiceProvider? _serviceProvider;
    private readonly Func<IServiceProvider, Task<IConnection>>? _factory;

    public RabbitMQHealthCheck(IConnection connection)
    {
        _connection = Guard.ThrowIfNull(connection);
    }

    public RabbitMQHealthCheck(IServiceProvider serviceProvider, Func<IServiceProvider, Task<IConnection>> factory)
    {
        _serviceProvider = Guard.ThrowIfNull(serviceProvider);
        _factory = Guard.ThrowIfNull(factory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checkDetails = new Dictionary<string, object>{
                { "health_check.task", "ready" },
                { "messaging.system", "rabbitmq" }
        };

        try
        {
            var connection = _connection ?? await _factory!(_serviceProvider!).ConfigureAwait(false);
            checkDetails.Add("server.address", connection.Endpoint.HostName);
            checkDetails.Add("server.port", connection.Endpoint.Port);
            checkDetails.Add("network.protocol.name", connection.Endpoint.Protocol.ApiName);
            checkDetails.Add("network.protocol.version", $"{connection.Endpoint.Protocol.MajorVersion}.{connection.Endpoint.Protocol.MinorVersion}.{connection.Endpoint.Protocol.Revision}");
            checkDetails.Add("network.local.port", connection.LocalPort);
            checkDetails.Add("network.remote.port", connection.RemotePort);

            await using var model = await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
