using HealthChecks.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="RabbitMQHealthCheck"/>.
/// </summary>
public static class RabbitMQHealthCheckBuilderExtensions
{
    private const string NAME = "rabbitmq";

    /// <summary>
    /// Add a health check for RabbitMQ services.
    /// </summary>
    /// <remarks>
    /// <paramref name="factory"/> will be called each time the healthcheck route is requested.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="factory">The action to get the RabbitMQ connection from the <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRabbitMQ(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IConnection>? factory = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                return new RabbitMQHealthCheck(sp, sp =>
                {
                    // Pass the factory to RabbitMQHealthCheck to ensure the factory is called
                    // within CheckHealthAsync. HealthCheckRegistration.Factory should not throw
                    // exceptions, which happen in the factory or getting the service.
                    var connection = factory?.Invoke(sp) ?? sp.GetRequiredService<IConnection>();
                    return Task.FromResult(connection);
                });
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for RabbitMQ services allowing for an asynchronous connection factory.
    /// </summary>
    /// <remarks>
    /// <paramref name="factory"/> will be called each time the healthcheck route is requested.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="factory">The asynchronous action to get the RabbitMQ connection from the <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'rabbitmq' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddRabbitMQ(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, Task<IConnection>> factory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new RabbitMQHealthCheck(sp, factory),
            failureStatus,
            tags,
            timeout));
    }
}
