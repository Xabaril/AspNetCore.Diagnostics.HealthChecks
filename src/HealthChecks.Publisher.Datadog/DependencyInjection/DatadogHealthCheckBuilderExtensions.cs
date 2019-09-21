using HealthChecks.Publisher.Datadog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StatsdClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatadogHealthCheckBuilderExtensions
    {
        /// <summary>
        /// Add a health check publisher for Datadog.
        /// </summary>
        /// <remarks>
        /// For each <see cref="HealthReport"/> published a custom service check indicating the health check status (OK - Healthy, WARNING - Degraded, CRITICAL - Unhealthy)
        /// and a metric indicating the total time the health check took to execute on milliseconds is sent to Datadog./>
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="serviceCheckName">Specifies the name of the custom check and metric that will be published to datadog. Example: "myservice.healthchecks".</param>
        /// <param name="datadogAgentName">Specified Datadog agent server name. Defaults to localhost address 127.0.0.1.</param>
        /// <param name="defaultTags">Specifies a collection of tags to send with the custom check and metric.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddDatadogPublisher(this IHealthChecksBuilder builder, string serviceCheckName, string datadogAgentName = "127.0.0.1", string[] defaultTags = default)
        {
            builder.Services
                .AddSingleton<IHealthCheckPublisher>(sp =>
                {
                    var dogStatsdService = new DogStatsdService();

                    dogStatsdService.Configure(new StatsdConfig
                    {
                        StatsdServerName = datadogAgentName
                    });

                    return new DatadogPublisher(dogStatsdService, serviceCheckName, defaultTags);
                });

            return builder;
        }
    }
}
