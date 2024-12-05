using HealthChecks.Qdrant;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Qdrant.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class QdrantHealthCheckBuilderExtensions
{
    private const string NAME = "qdrant";

    /// <summary>
    /// Add a health check for Qdrant services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="QdrantClient" /> instance.
    /// When not provided, <see cref="QdrantClient" /> is simply resolved from <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'qdrant' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddQdrant(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, QdrantClient>? clientFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new QdrantHealthCheck(clientFactory?.Invoke(sp) ?? sp.GetRequiredService<QdrantClient>()),
            failureStatus,
            tags,
            timeout));
    }
}
