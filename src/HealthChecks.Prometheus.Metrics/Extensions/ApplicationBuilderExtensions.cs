using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

namespace HealthChecks.Prometheus.Metrics.Extensions 
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