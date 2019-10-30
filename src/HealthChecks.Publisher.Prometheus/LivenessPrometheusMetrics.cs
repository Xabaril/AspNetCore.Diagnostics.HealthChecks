using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Prometheus.Advanced;
using System.IO;

namespace HealthChecks.Publisher.Prometheus
{
    public abstract class LivenessPrometheusMetrics
    {
        protected const string ContentType = "text/plain; version=0.0.4";
        private const string HealthCheckLabelName = "healthcheck";
        private readonly Gauge _healthChecksDuration;
        private readonly Gauge _healthChecksResult;
        protected readonly ICollectorRegistry Registry;

        internal LivenessPrometheusMetrics()
        {
            _healthChecksResult = Metrics.CreateGauge("healthcheck",
                "Shows raw health check status (0 = Unhealthy, 1 = Degraded, 2 = Healthy)", new GaugeConfiguration
                {
                    LabelNames = new[] {HealthCheckLabelName},
                    SuppressInitialValue = false
                });

            _healthChecksDuration = Metrics.CreateGauge("healthcheck_duration_seconds",
                "Shows duration of the health check execution in seconds",
                new GaugeConfiguration
                {
                    LabelNames = new[] {HealthCheckLabelName},
                    SuppressInitialValue = false
                });

            Registry = DefaultCollectorRegistry.Instance;
        }
        protected void WriteMetricsFromHealthReport(HealthReport report)
        {
            foreach (var reportEntry in report.Entries)
            {
                _healthChecksResult.Labels(reportEntry.Key).
                    Set((double)reportEntry.Value.Status);

                _healthChecksDuration.Labels(reportEntry.Key)
                    .Set(reportEntry.Value.Duration.TotalSeconds);
            }
        }
        protected static Stream CollectionToStreamWriter(ICollectorRegistry registry)
        {
            var metrics = registry.CollectAll();
            var stream = new MemoryStream();
            ScrapeHandler.ProcessScrapeRequest(metrics, ContentType, stream);

            stream.Position = 0;
            return stream;
        }
    }
}