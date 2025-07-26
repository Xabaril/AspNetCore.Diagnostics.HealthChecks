using Amqp;
using Microsoft.Extensions.Diagnostics.HealthChecks;


namespace HealthChecks.Activemq;

/// <summary>
/// A health check for ActiveMQ services.
/// </summary>
public class ActiveMqHealthCheck : IHealthCheck
{
    private readonly IConnection? _connection;

    public ActiveMqHealthCheck(IConnection connection)
    {
        _connection = Guard.ThrowIfNull(connection);
    }
    /// <summary>
    /// CheckHealthAsync
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection == null)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: "Connection is null."));
            }

            if (_connection.IsClosed)
            {
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, description: "Connection is not established."));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }
}
