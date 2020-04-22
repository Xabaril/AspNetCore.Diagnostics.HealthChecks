using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace HealthChecks.Prometheus.Metrics.Extensions {
    public static class PrometheusHealthCheckMiddleware {
        public static IApplicationBuilder UseHealthChecksPrometheusExporter(this IApplicationBuilder applicationBuilder, string metricsEndpointName = "/healthmetrics") 
        {
            applicationBuilder.UseHealthChecks(metricsEndpointName, new HealthCheckOptions 
            {
                ResponseWriter = PrometheusResponseWriter.WritePrometheusResultText
            });
            return applicationBuilder;
        }
    }
}