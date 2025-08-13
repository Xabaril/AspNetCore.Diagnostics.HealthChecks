using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
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

    [Fact]
    public async Task send_availabilityTelemetry_when_trackAsAvailability_is_true_and_detailedReport_is_false()
    {
        var telemetryChannel = Substitute.For<ITelemetryChannel>();
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = telemetryChannel,
            DisableTelemetry = false
        };
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var options = Substitute.For<IOptions<TelemetryConfiguration>>();
        var publisher = Substitute.ForPartsOf<ApplicationInsightsPublisher>(options, "ConnectionString", false, false, true);
        publisher.When(svc => svc.GetOrCreateTelemetryClient()).DoNotCallBase();
        publisher.GetOrCreateTelemetryClient().Returns(telemetryClient);
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.FromSeconds(5));

        await publisher.PublishAsync(report, CancellationToken.None);

        telemetryChannel
            .Received()
            .Send( Arg.Is<ITelemetry>(i => i is AvailabilityTelemetry &&
                                           (i as AvailabilityTelemetry)!.Duration == report.TotalDuration &&
                                           (i as AvailabilityTelemetry)!.RunLocation == Environment.MachineName &&
                                           (i as AvailabilityTelemetry)!.Success &&
                                           (i as AvailabilityTelemetry)!.Properties.Count == 1
                                           ));
    }

    [Fact]
    public async Task send_eventTelemetry_when_trackAsAvailability_and_detailedReport_is_false()
    {
        var telemetryChannel = Substitute.For<ITelemetryChannel>();
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = telemetryChannel,
            DisableTelemetry = false
        };
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var options = Substitute.For<IOptions<TelemetryConfiguration>>();
        var publisher = Substitute.ForPartsOf<ApplicationInsightsPublisher>(options, "ConnectionString", false, false, false);
        publisher.When(svc => svc.GetOrCreateTelemetryClient()).DoNotCallBase();
        publisher.GetOrCreateTelemetryClient().Returns(telemetryClient);
        var report = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.FromSeconds(5));

        await publisher.PublishAsync(report, CancellationToken.None);

        telemetryChannel
            .Received()
            .Send( Arg.Is<ITelemetry>(i => i is EventTelemetry &&
                                           (i as EventTelemetry)!.Properties.Count == 2 &&
                                           (i as EventTelemetry)!.Metrics.Count == 2
            ));
    }

    [Fact]
    public async Task send_availabilityTelemetry_when_trackAsAvailability_and_detailedReport_is_true()
    {
        var telemetryChannel = Substitute.For<ITelemetryChannel>();
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = telemetryChannel,
            DisableTelemetry = false
        };
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var options = Substitute.For<IOptions<TelemetryConfiguration>>();
        var publisher = Substitute.ForPartsOf<ApplicationInsightsPublisher>(options, "ConnectionString", true, false, true);
        publisher.When(svc => svc.GetOrCreateTelemetryClient()).DoNotCallBase();
        publisher.GetOrCreateTelemetryClient().Returns(telemetryClient);
        var reportEntry1 = new HealthReportEntry(HealthStatus.Healthy, null, TimeSpan.FromSeconds(2), null, new Dictionary<string, object>() );
        var report = new HealthReport(new Dictionary<string, HealthReportEntry> { { "reportEntry1", reportEntry1 } }, TimeSpan.FromSeconds(5));

        await publisher.PublishAsync(report, CancellationToken.None);

        telemetryChannel
            .Received()
            .Send( Arg.Is<ITelemetry>(i => i is AvailabilityTelemetry &&
                                           (i as AvailabilityTelemetry)!.Duration == reportEntry1.Duration &&
                                           (i as AvailabilityTelemetry)!.RunLocation == Environment.MachineName &&
                                           (i as AvailabilityTelemetry)!.Success &&
                                           (i as AvailabilityTelemetry)!.Message == null &&
                                           (i as AvailabilityTelemetry)!.Properties.Count == 2
            ));
    }

    [Fact]
    public async Task send_eventTelemetry_when_trackAsAvailability_is_false_and_detailedReport_is_true()
    {
        var telemetryChannel = Substitute.For<ITelemetryChannel>();
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = telemetryChannel,
            DisableTelemetry = false
        };
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var options = Substitute.For<IOptions<TelemetryConfiguration>>();
        var publisher = Substitute.ForPartsOf<ApplicationInsightsPublisher>(options, "ConnectionString", true, false, false);
        publisher.When(svc => svc.GetOrCreateTelemetryClient()).DoNotCallBase();
        publisher.GetOrCreateTelemetryClient().Returns(telemetryClient);
        var reportEntry1 = new HealthReportEntry(HealthStatus.Healthy, null, TimeSpan.FromSeconds(2), null, new Dictionary<string, object>() );
        var report = new HealthReport(new Dictionary<string, HealthReportEntry> { { "reportEntry1", reportEntry1 } }, TimeSpan.FromSeconds(5));

        await publisher.PublishAsync(report, CancellationToken.None);

        telemetryChannel
            .Received()
            .Send( Arg.Is<ITelemetry>(i => i is EventTelemetry &&
                                           (i as EventTelemetry)!.Properties.Count == 2 &&
                                           (i as EventTelemetry)!.Metrics.Count == 2
            ));
    }
}
