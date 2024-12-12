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
        try
        {
            var connection = _connection ?? await _factory!(_serviceProvider!).ConfigureAwait(false);

            await using var model = await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
