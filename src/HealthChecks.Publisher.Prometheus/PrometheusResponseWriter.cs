using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.Prometheus
{
    public sealed class PrometheusResponseWriter : LivenessPrometheusMetrics
    {
        public static async Task WritePrometheusResultText(HttpContext context, HealthReport report,
            bool alwaysReturnHttp200Ok = false)
        {
            var instance = new PrometheusResponseWriter();
            instance.WriteMetricsFromHealthReport(report);
            var resultStream = CollectionToStreamWriter(instance.Registry);
            var result = await new StreamContent(resultStream).ReadAsStringAsync();
            context.Response.ContentType = ContentType;
            if (alwaysReturnHttp200Ok)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
            }

            await context.Response.WriteAsync(result, Encoding.UTF8);
        }
    }
}