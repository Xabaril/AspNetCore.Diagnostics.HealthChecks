using HealthChecks.Publisher.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationInsightsHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check publisher for Application Insights.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to Application Insights with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c>
    /// indicating the health check status (1 - Healthy, 0 - Unhealthy) and the total time the health check took to execute in milliseconds.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">Specified Application Insights connection string. Optional. If <c>null</c> <see cref="TelemetryConfiguration"/> is resolved from DI container.</param>
    /// <param name="saveDetailedReport">Specifies if save an Application Insights event for each HealthCheck or just save one event with the global status for all the HealthChecks. Optional.</param>
    /// <param name="excludeHealthyReports">Specifies if save an Application Insights event only for reports indicating an unhealthy status.</param>
    /// <param name="trackAsAvailability">Specifies if healthCheck result should be tracked as availability event. If false, healthCheck result will be tracked as custom event.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddApplicationInsightsPublisher(
        this IHealthChecksBuilder builder,
        string? connectionString = default,
        bool saveDetailedReport = false,
        bool excludeHealthyReports = false,
        bool trackAsAvailability = false)
    {
        builder.Services
           .AddSingleton<IHealthCheckPublisher>(sp =>
           {
               var telemetryConfigurationOptions = sp.GetService<IOptions<TelemetryConfiguration>>();
               return new ApplicationInsightsPublisher(telemetryConfigurationOptions, connectionString, saveDetailedReport, excludeHealthyReports, trackAsAvailability);
           });

        return builder;
    }
}
