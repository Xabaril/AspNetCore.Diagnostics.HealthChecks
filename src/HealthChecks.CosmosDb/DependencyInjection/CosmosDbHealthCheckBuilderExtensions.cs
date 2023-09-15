using HealthChecks.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureCosmosDbHealthCheck"/>.
/// </summary>
public static class CosmosDbHealthCheckBuilderExtensions
{
    private const string HEALTH_CHECK_NAME = "azure_cosmosdb";

    /// <summary>
    /// Add a health check for Azure Cosmos DB by registering <see cref="AzureCosmosDbHealthCheck"/> for given <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add <see cref="HealthCheckRegistration"/> to.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="CosmosClient" /> instance.
    /// When not provided, <see cref="CosmosClient" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional factory to obtain <see cref="AzureCosmosDbHealthCheckOptions"/> used by the health check.
    /// When not provided, defaults are used.
    /// </param>
    /// <param name="healthCheckName">The health check name. Optional. If <c>null</c> the name 'azure_cosmosdb' will be used.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureCosmosDB(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, CosmosClient>? clientFactory = default,
        Func<IServiceProvider, AzureCosmosDbHealthCheckOptions>? optionsFactory = default,
        string? healthCheckName = HEALTH_CHECK_NAME,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
           string.IsNullOrEmpty(healthCheckName) ? HEALTH_CHECK_NAME : healthCheckName!,
           sp => new AzureCosmosDbHealthCheck(
                    cosmosClient: clientFactory?.Invoke(sp) ?? sp.GetRequiredService<CosmosClient>(),
                    options: optionsFactory?.Invoke(sp)),
           failureStatus,
           tags,
           timeout));
    }
}
