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

    public static IHealthChecksBuilder AddNeo4jClient(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, IBoltGraphClient> graphClientFactory,
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
