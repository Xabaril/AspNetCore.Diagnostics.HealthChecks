using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureSearch.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureSearchHealthCheck"/>.
/// </summary>
public static class AzureSearchHealthCheckBuilderExtensions
{
    private const string NAME = "azuresearch";

    /// <summary>
    /// Add a health check for Azure Search.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the Azure Search connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuresearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureSearch(
        this IHealthChecksBuilder builder,
        Action<AzureSearchOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var azureSearchOptions = new AzureSearchOptions();
        setup?.Invoke(azureSearchOptions);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new AzureSearchHealthCheck(azureSearchOptions),
           failureStatus,
           tags,
           timeout));
    }
}
