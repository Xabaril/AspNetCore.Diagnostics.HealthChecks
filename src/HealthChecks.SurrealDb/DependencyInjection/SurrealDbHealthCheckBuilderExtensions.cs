using HealthChecks.SurrealDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="SurrealDbHealthCheck"/>.
/// </summary>
public static class SurrealDbHealthCheckBuilderExtensions
{
    private const string NAME = "surrealdb";

    /// <summary>
    /// Add a health check for SurrealDB.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="factory">
    /// An optional factory to obtain <see cref="ISurrealDbClient" /> instance.
    /// When not provided, <see cref="ISurrealDbClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'surrealdb' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSurreal(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, ISurrealDbClient>? factory = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => Factory(sp, factory),
            failureStatus,
            tags,
            timeout));

        static SurrealDbHealthCheck Factory(IServiceProvider sp, Func<IServiceProvider, ISurrealDbClient>? factory)
        {
            // The user might have registered a factory for SurrealDbClient type, but not for the abstraction (ISurrealDbClient).
            // That is why we try to resolve ISurrealDbClient first.
            ISurrealDbClient client = factory?.Invoke(sp) ?? sp.GetService<ISurrealDbClient>() ?? sp.GetRequiredService<SurrealDbClient>();
            return new(client);
        }
    }
}
