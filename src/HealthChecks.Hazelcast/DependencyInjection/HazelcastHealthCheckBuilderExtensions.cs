using HealthChecks.Hazelcast;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="HazelcastHealthCheck"/>.
/// </summary>
public static class HazelcastHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check for Hazelcast.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">Action to configure <see cref="HazelcastHealthCheckOptions"/></param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'hazelcast' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns></returns>
    public static IHealthChecksBuilder AddHazelcast(
        this IHealthChecksBuilder builder,
        Action<HazelcastHealthCheckOptions> setup,
        string? name = default,
        HealthStatus failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new HazelcastHealthCheckOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? "hazelcast",
            _ => new HazelcastHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Hazelcast.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="options">Options for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'hazelcast' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns></returns>
    public static IHealthChecksBuilder AddHazelcast(
        this IHealthChecksBuilder builder,
        HazelcastHealthCheckOptions options,
        string? name = default,
        HealthStatus failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? "hazelcast",
            _ => new HazelcastHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
