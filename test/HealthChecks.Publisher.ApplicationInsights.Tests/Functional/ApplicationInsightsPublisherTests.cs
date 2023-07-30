using Microsoft.ApplicationInsights.Extensibility;
using NSubstitute;

namespace HealthChecks.Publisher.ApplicationInsights.Tests.Functional;

public class application_insights_publisher_should
{
    [Fact]
    public void throw_when_neither_connection_string_nor_telemetry_configuration_is_provided()
    {
        var options = Substitute.For<IOptions<TelemetryConfiguration>>();
        var publisher = new ApplicationInsightsPublisher(options, connectionString: null);
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        var publish = () => publisher.PublishAsync(report, default);

        publish.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("A connection string or TelemetryConfiguration must be set!");
    }
}
