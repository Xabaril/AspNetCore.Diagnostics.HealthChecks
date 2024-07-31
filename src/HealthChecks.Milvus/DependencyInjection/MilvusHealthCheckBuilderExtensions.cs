using HealthChecks.Milvus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Milvus.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="MilvusHealthCheck"/>.
/// </summary>
public static class MilvusHealthCheckBuilderExtensions
{
    private const string NAME = "milvus";

    /// <summary>
    /// Add a health check for Milvus services.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">The Redis connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'milvus' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddMilvus(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, MilvusClient>? clientFactory = default,
        string? name = NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new MilvusHealthCheck(clientFactory?.Invoke(sp) ?? sp.GetRequiredService<MilvusClient>()),
            failureStatus,
            tags,
            timeout));
    }
}
