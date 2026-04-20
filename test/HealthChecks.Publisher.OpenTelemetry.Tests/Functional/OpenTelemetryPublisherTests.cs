using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace HealthChecks.Publisher.OpenTelemetry.Tests.Functional;

public class open_telemetry_publisher_should
{
    [Theory]
    [InlineData(HealthStatus.Healthy, 1)]
    [InlineData(HealthStatus.Degraded, 0.5)]
    [InlineData(HealthStatus.Unhealthy, 0)]
    public void publish_report_as_metrics(HealthStatus healthStatus, double metricValue)
    {
        // Arrange
        const string meterName = "Microsoft.AspNetCore.Diagnostics.HealthChecks";
        const string nameTagKey = "health_check.name";
        const string statusMetricName = "health_check.status";
        const string durationMetricName = "health_check.duration";
        const string healthCheckName = "unit_test";

        var exportedItems = new List<Metric>();

        var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddInMemoryExporter(exportedItems)
            .AddMeter(meterName)
            .Build();

        var duration = TimeSpan.FromSeconds(9.5);

        var healthCheck = new HealthReportEntry(
            healthStatus,
            description: null,
            duration: duration,
            exception: null,
            data: null);

        var healthChecks = new Dictionary<string, HealthReportEntry> { { healthCheckName, healthCheck } };

        var sut = new OpenTelemetryPublisher();

        // Act
        sut.PublishAsync(new HealthReport(healthChecks, duration), CancellationToken.None);

        // Assert
        meterProvider.ForceFlush();
        exportedItems.Count.ShouldBe(2);
        exportedItems.ShouldAllBe(x => x.MeterName == meterName);

        var statusMetric = exportedItems.Find(x => x.Name == statusMetricName);
        statusMetric.ShouldNotBeNull();

        var statusPoint = GetFirstMetricPoint(statusMetric.GetMetricPoints());
        statusPoint.ShouldNotBeNull();
        statusPoint.Value.GetGaugeLastValueDouble().ShouldBe(metricValue);

        string? statusNameTag = GetTagByKey(statusPoint.Value, nameTagKey);
        statusNameTag.ShouldNotBeNull();
        statusNameTag.ShouldBe(healthCheckName);

        var durationMetric = exportedItems.Find(x => x.Name == durationMetricName);
        durationMetric.ShouldNotBeNull();

        var durationPoint = GetFirstMetricPoint(durationMetric.GetMetricPoints());
        durationPoint.ShouldNotBeNull();
        durationPoint.Value.GetGaugeLastValueDouble().ShouldBe(duration.TotalSeconds);

        string? durationNameTag = GetTagByKey(durationPoint.Value, nameTagKey);
        durationNameTag.ShouldNotBeNull();
        durationNameTag.ShouldBe(healthCheckName);
    }

    private static MetricPoint? GetFirstMetricPoint(MetricPointsAccessor accessor)
    {
        foreach (var metricPoint in accessor)
        {
            return metricPoint;
        }

        return null;
    }

    private static string? GetTagByKey(MetricPoint metricPoint, string key)
    {
        foreach (var tag in metricPoint.Tags)
        {
            if (tag.Key == key)
            {
                return tag.Value?.ToString();
            }
        }

        return null;
    }
}
