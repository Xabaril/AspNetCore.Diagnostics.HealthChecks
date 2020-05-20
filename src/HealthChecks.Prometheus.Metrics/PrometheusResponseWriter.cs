using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Prometheus.Metrics
{
    public sealed class PrometheusResponseWriter : LivenessPrometheusMetrics
    {
        public static async Task WritePrometheusResultText(HttpContext context, HealthReport report)
        {
            var instance = new PrometheusResponseWriter();
            instance.WriteMetricsFromHealthReport(report);

            context.Response.ContentType = ContentType;
            await instance.Registry.CollectAndExportAsTextAsync(context.Response.Body, context.RequestAborted);
        }
    }
}