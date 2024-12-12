using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;

namespace HealthChecks.Publisher.Prometheus;

public abstract class LivenessPrometheusMetrics
{
    protected const string CONTENT_TYPE = PrometheusConstants.TextContentTypeWithVersionAndEncoding;
    private const string HEALTH_CHECK_LABEL_NAME = "healthcheck";
    private readonly Gauge _healthChecksDuration;
    private readonly Gauge _healthChecksResult;
    protected readonly CollectorRegistry Registry;

    internal LivenessPrometheusMetrics()
    {
        Registry = Metrics.DefaultRegistry;
        var factory = Metrics.WithCustomRegistry(Registry);

        _healthChecksResult = factory.CreateGauge("healthcheck",
            "Shows raw health check status (0 = Unhealthy, 1 = Degraded, 2 = Healthy)", new GaugeConfiguration
            {
                LabelNames = [HEALTH_CHECK_LABEL_NAME],
                SuppressInitialValue = false
            });

        _healthChecksDuration = factory.CreateGauge("healthcheck_duration_seconds",
            "Shows duration of the health check execution in seconds",
            new GaugeConfiguration
            {
                LabelNames = [HEALTH_CHECK_LABEL_NAME],
                SuppressInitialValue = false
            });
    }
    protected void WriteMetricsFromHealthReport(HealthReport report)
    {
        foreach (var reportEntry in report.Entries)
        {
            _healthChecksResult.WithLabels(reportEntry.Key).
                Set((double)reportEntry.Value.Status);

            _healthChecksDuration.WithLabels(reportEntry.Key)
                .Set(reportEntry.Value.Duration.TotalSeconds);
        }
    }
}
