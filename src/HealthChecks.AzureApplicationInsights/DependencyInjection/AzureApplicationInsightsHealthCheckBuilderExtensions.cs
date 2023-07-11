using HealthChecks.AzureApplicationInsights;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureApplicationInsightsHealthCheck"/>.
/// </summary>
public static class AzureApplicationInsightsHealthCheckBuilderExtensions
{
    internal const string AZUREAPPLICATIONINSIGHTS_NAME = "azureappinsights";

    /// <summary>
    /// Add a health check for specified Azure Application Insights.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="instrumentationKey">The azure app insights instrumentation key.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureappinsights' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
    /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddAzureApplicationInsights(
        this IHealthChecksBuilder builder,
        string instrumentationKey,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var registrationName = name ?? AZUREAPPLICATIONINSIGHTS_NAME;
        builder.Services.AddHttpClient(registrationName);

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp => new AzureApplicationInsightsHealthCheck(instrumentationKey, sp.GetRequiredService<IHttpClientFactory>()),
            failureStatus,
            tags,
            timeout));
    }
}
