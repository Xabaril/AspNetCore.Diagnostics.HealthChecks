using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.ApplicationInsights.DependencyInjection
{
    
    public static class ApplicationInsightsHealthCheckBuilderExtensions
    {
        /// <summary>
        /// Add a health check publisher for Application Insights.
        /// </summary>
        /// <remarks>
        /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to Application Insights with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c>
        /// indicating the health check status ( 1 Healthy 0 Unhealthy)  and the total time the health check took to execute on milliseconds./>
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="instrumentationKey">Specified Application Insights instrumentation key. Optional. If <c>null</c> TelemetryConfiguration.Active is used.</param>
        /// <param name="saveDetailedReport">Specifies if save an Application Isnghts event for each HealthCheck or just save one event with the global status for all the HealthChecks. Optional: If <c>true</c> saves an Application Insights event for each HEalthCheck</c></param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddApplicationInsightsPublisher(this IHealthChecksBuilder builder, string instrumentationKey = default, bool saveDetailedReport = false)
        {
            builder.Services
               .AddSingleton<IHealthCheckPublisher>(sp =>
               {
                   return new ApplicationInsightsPublisher(instrumentationKey, saveDetailedReport);
               });

            return builder;
        }
    }
}
