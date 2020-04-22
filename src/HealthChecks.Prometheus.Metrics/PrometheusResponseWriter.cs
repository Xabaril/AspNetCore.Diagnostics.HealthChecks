using System.Net.Http;
using System.Text;
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

            using (var resultStream = CollectionToStreamWriter(instance.Registry))
            {
                var content = await new StreamContent(resultStream)
                   .ReadAsStringAsync();

                context.Response.ContentType = ContentType;

                await context.Response.WriteAsync(content, Encoding.UTF8);
            }
        }
    }
}