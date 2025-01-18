using HealthChecks.Neo4jClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4jClient;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="Neo4jClientHealthCheck"/>.
/// </summary>
public static class Neo4jClientHealthCheckBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "neo4j";

    /// <summary>
    /// Add a health check for Neo4j databases.
    /// </summary>
    /// <param name="builder">The extension for <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="graphClientFactory">A factory to build <see cref="IGraphClient"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'neo4j' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNeo4jClient(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IGraphClient> graphClientFactory,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name ?? HEALTH_CHECK_NAME,
            sp =>
            {
                var graphClient = graphClientFactory(sp);
                var options = new Neo4jClientHealthCheckOptions(graphClient);

                return new Neo4jClientHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout
            );

        return builder.Add(healthCheckRegistration);
    }

    /// <summary>
    /// Add a health check for Neo4j databases.
    /// </summary>
    /// <param name="builder">The extension for <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="healthCheckOptions"><see cref="Neo4jClientHealthCheckOptions"/> instance for health check.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'neo4j' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddNeo4jClient(
        this IHealthChecksBuilder builder,
        Neo4jClientHealthCheckOptions healthCheckOptions,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name ?? HEALTH_CHECK_NAME,
            _ => new Neo4jClientHealthCheck(healthCheckOptions),
            failureStatus,
            tags,
            timeout
            );

        return builder.Add(healthCheckRegistration);
    }
}
