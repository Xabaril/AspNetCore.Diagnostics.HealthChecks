using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;

namespace HealthChecks.Publisher.Prometheus
{
    public sealed class PrometheusResponseWriter : LivenessPrometheusMetrics
    {
        public static async Task WritePrometheusResultText(HttpContext context, HealthReport report, bool alwaysReturnHttp200Ok)
        {
            var instance = new PrometheusResponseWriter();
            instance.WriteMetricsFromHealthReport(report);

            context.Response.ContentType = ContentType;
            if (alwaysReturnHttp200Ok)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
            }

            await instance.Registry.CollectAndExportAsTextAsync(context.Response.Body, context.RequestAborted);
        }

        public static async Task WritePrometheusResultText(HttpContext context, HealthReport report) 
        {
            await WritePrometheusResultText(context, report, false);
        }
    }
}