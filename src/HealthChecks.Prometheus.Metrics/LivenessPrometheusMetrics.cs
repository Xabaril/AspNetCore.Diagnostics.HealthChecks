using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace HealthChecks.Prometheus.Metrics
{
    public abstract class LivenessPrometheusMetrics
    {
        protected const string ContentType = "text/plain; version=0.0.4";
        private const string HealthCheckLabelName = "healthcheck";
        private readonly Gauge _healthChecksDuration;
        private readonly Gauge _healthChecksResult;
        protected readonly CollectorRegistry Registry;

        internal LivenessPrometheusMetrics()
        {
            Registry = global::Prometheus.Metrics.DefaultRegistry;
            var factory = global::Prometheus.Metrics.WithCustomRegistry(Registry);

            _healthChecksResult = factory.CreateGauge("healthcheck",
                "Shows raw health check status (0 = Unhealthy, 1 = Degraded, 2 = Healthy)", new GaugeConfiguration
                {
                    LabelNames = new[] {HealthCheckLabelName},
                    SuppressInitialValue = false
                });

            _healthChecksDuration = factory.CreateGauge("healthcheck_duration_seconds",
                "Shows duration of the health check execution in seconds",
                new GaugeConfiguration
                {
                    LabelNames = new[] {HealthCheckLabelName},
                    SuppressInitialValue = false
                });
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
    }
}