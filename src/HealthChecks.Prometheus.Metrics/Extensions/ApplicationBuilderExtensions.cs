using HealthChecks.Prometheus.Metrics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder
{
    public static class PrometheusHealthCheckMiddleware 
    {
        public static IApplicationBuilder UseHealthChecksPrometheusExporter(this IApplicationBuilder applicationBuilder, PathString endpoint) 
        {
            applicationBuilder.UseHealthChecks(endpoint, new HealthCheckOptions 
            {
                ResponseWriter = PrometheusResponseWriter.WritePrometheusResultText
            });
            return applicationBuilder;
        }
    }
}