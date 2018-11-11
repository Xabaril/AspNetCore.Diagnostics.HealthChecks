using System.IO;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using Prometheus.Advanced;

namespace HealthChecks.Publisher.Prometheus
{
    public abstract class LivenessPrometheusMetrics
    {
        protected const string ContentType = "text/plain; version=0.0.4";
        private const string HealthCheckLabelName = "healthcheck";
        private readonly Gauge _livenessDuration;
        private readonly Gauge _livenessResult;
        protected readonly ICollectorRegistry Registry;

        internal LivenessPrometheusMetrics()
        {
            _livenessResult = Metrics.CreateGauge("liveness",
                "Shows raw health check liveness (0 = Unhealthy, 1 = Degraded, 2 = Healthy)", new GaugeConfiguration
                {
                    LabelNames = new[] {HealthCheckLabelName},
                    SuppressInitialValue = false
                });

            _livenessDuration = Metrics.CreateGauge("liveness_duration_seconds",
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
                _livenessResult.Labels(reportEntry.Key).Set((double) reportEntry.Value.Status);
                _livenessDuration.Labels(reportEntry.Key).Set(reportEntry.Value.Duration.TotalSeconds);
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