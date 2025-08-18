using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.OpenTelemetry;

public sealed class OpenTelemetryPublisher : IHealthCheckPublisher
{
    internal const string METER_NAME = "Microsoft.AspNetCore.Diagnostics.HealthChecks";

    private const string HEALTH_CHECK_NAME = "health_check.name";

    private const string HEALTH_CHECK_STATUS_NAME = "health_check.status";
    private const string HEALTH_CHECK_STATUS_DESCRIPTION = "ASP.NET Core health check status (0 == Unhealthy, 0.5 == Degraded, 1 == Healthy)";

    private const string HEALTH_CHECK_DURATION_NAME = "health_check.duration";
    private const string HEALTH_CHECK_DURATION_DESCRIPTION = "Shows duration of the health check execution in seconds";

    private HealthReport? _lastReport;

    public OpenTelemetryPublisher()
    {
        var meter = new Meter(METER_NAME);

        meter.CreateObservableGauge(
            HEALTH_CHECK_STATUS_NAME,
            ObserveStatus,
            unit: "status",
            description: HEALTH_CHECK_STATUS_DESCRIPTION);

        meter.CreateObservableGauge(
            HEALTH_CHECK_DURATION_NAME,
            ObserveDuration,
            unit: "seconds",
            description: HEALTH_CHECK_DURATION_DESCRIPTION);
    }

    public Task PublishAsync(
        HealthReport report,
        CancellationToken cancellationToken)
    {
        _lastReport = report;
        return Task.CompletedTask;
    }

    private IEnumerable<Measurement<double>> ObserveStatus()
    {
        if (_lastReport is null)
        {
            yield break;
        }

        foreach (var (key, entry) in _lastReport.Entries)
        {
            yield return new Measurement<double>(
                HealthStatusToMetricValue(entry.Status),
                new KeyValuePair<string, object?>(HEALTH_CHECK_NAME, key));
        }
    }

    private IEnumerable<Measurement<double>> ObserveDuration()
    {
        if (_lastReport is null)
        {
            yield break;
        }

        foreach (var (key, entry) in _lastReport.Entries)
        {
            yield return new Measurement<double>(
                entry.Duration.TotalSeconds,
                new KeyValuePair<string, object?>(HEALTH_CHECK_NAME, key));
        }
    }

    private static double HealthStatusToMetricValue(HealthStatus status)
        => status switch
        {
            HealthStatus.Unhealthy => 0,
            HealthStatus.Degraded => 0.5,
            HealthStatus.Healthy => 1,
            _ => throw new NotSupportedException($"Unexpected HealthStatus value: {status}"),
        };
}
