using Amqp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Activemq;

/// <summary>
/// Extension methods to configure <see cref="ActiveMqHealthCheck"/>.
/// </summary>
public static class ActiveMQHealthCheckBuilderExtensions
{
    private const string NAME = "activemq";

    public static IHealthChecksBuilder AddActiveMQ(
        this IHealthChecksBuilder builder,
        string? name,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // Register the health check service
        builder.Services.AddKeyedSingleton<ActiveMqHealthCheck>(name);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            provider => provider.GetRequiredKeyedService<ActiveMqHealthCheck>(name),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "activemq" },
            timeout));
    }

    public static IHealthChecksBuilder AddActiveMQ(
        this IHealthChecksBuilder builder,
        string? name,
        IConnection connection,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        // Register the health check service
        builder.Services.AddKeyedSingleton<ActiveMqHealthCheck>(name, (provider, _) => new ActiveMqHealthCheck(connection));

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            provider => provider.GetRequiredKeyedService<ActiveMqHealthCheck>(name),
            failureStatus ?? HealthStatus.Unhealthy,
            tags ?? new[] { "activemq" },
            timeout));
    }

}
