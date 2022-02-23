using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Prometheus.Metrics
{
    public sealed class PrometheusResponseWriter : LivenessPrometheusMetrics
    {
#pragma warning disable IDE1006 // Naming Styles
        public static async Task WritePrometheusResultText(HttpContext context, HealthReport report) //TODO: change public API
#pragma warning restore IDE1006 // Naming Styles
        {
            var instance = new PrometheusResponseWriter();
            instance.WriteMetricsFromHealthReport(report);

            context.Response.ContentType = CONTENT_TYPE;
            await instance.Registry.CollectAndExportAsTextAsync(context.Response.Body, context.RequestAborted);
        }
    }
}
