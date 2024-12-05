using HealthChecks.Nats;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NATS.Client.Core;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="NatsHealthCheck"/>.
/// </summary>
public static class NatsHealthCheckBuilderExtensions
{
    internal const string NAME = "nats";

    /// <summary>
    /// Add a health check for Nats services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="INatsConnection" /> instance.
    /// When not provided, <see cref="INatsConnection" /> is simply resolved from <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'nats' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNats(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, INatsConnection>? clientFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new NatsHealthCheck(clientFactory?.Invoke(sp) ?? sp.GetRequiredService<INatsConnection>()),
            failureStatus,
            tags,
            timeout));
    }
}
