using HealthChecks.Publisher.Prometheus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PrometheusGatewayHealthCheckBuilderExtensions
    {
        /// <summary>
        ///     Add a health check publisher for Prometheus Gateway.
        /// </summary>
        /// <remarks>
        ///     For each <see cref="HealthReport" /> published a new metric value indicating the health check status (2 Healthy, 1
        ///     Degraded, 0 Unhealthy)  and the total time the health check took to execute on seconds.
        /// </remarks>
        /// <param name="builder">The <see cref="IHealthChecksBuilder" />.</param>
        /// <param name="endpoint">Endpoint url e.g. http://myendpoint:9091</param>
        /// <param name="job"> The job name the series can be filtered on (typically the application name).</param>
        /// <param name="instance">If there are multiple instances.</param>
        /// <returns>The <see cref="IHealthChecksBuilder" />.</returns>
        public static IHealthChecksBuilder AddPrometheusGatewayPublisher(this IHealthChecksBuilder builder,
            string endpoint, string job, string instance = null)
        {
            builder.Services
                .AddHttpClient();

            builder.Services
                .AddSingleton<IHealthCheckPublisher>(sp =>
                {
                    return new PrometheusGatewayPublisher(() => sp.GetRequiredService<IHttpClientFactory>().CreateClient(), endpoint, job, instance);
                });

            return builder;
        }
    }
}