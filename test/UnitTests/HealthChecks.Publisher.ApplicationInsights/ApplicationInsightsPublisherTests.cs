using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.Publisher.ApplicationInsights;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.HealthChecks.Publisher.ApplicationInsights
{
    public class application_insights_publisher_should
    {
        private ApplicationInsightsPublisher publisher;
        private readonly HealthReport report;
        private readonly CancellationToken cancellationToken = CancellationToken.None;
        private readonly FakeTelemetryChannel telemetryChannel;
        private ITestOutputHelper testOutputHelper;

        public application_insights_publisher_should(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;

            telemetryChannel = new FakeTelemetryChannel();

#pragma warning disable CS0618 // Type or member is obsolete
            TelemetryConfiguration.Active.TelemetryChannel = telemetryChannel;
#pragma warning restore CS0618 // Type or member is obsolete

            Dictionary<string, HealthReportEntry> entries = new Dictionary<string, HealthReportEntry>();

            var properties = new Dictionary<string, object>
            {
                { "foo", "bar" }
            };

            entries.Add("healthy", new HealthReportEntry(HealthStatus.Healthy, "healthy", TimeSpan.FromMilliseconds(25), exception: null, data: properties));
            entries.Add("unhealthy", new HealthReportEntry(HealthStatus.Unhealthy, "unhealthy", TimeSpan.FromMilliseconds(25), exception: new ArgumentNullException(), data: properties));
            entries.Add("degraded", new HealthReportEntry(HealthStatus.Degraded, "degraded", TimeSpan.FromMilliseconds(25), exception: null, data: properties));
            entries.Add("degraded_with_exception", new HealthReportEntry(HealthStatus.Degraded, "degraded with exception", TimeSpan.FromMilliseconds(25), exception: new NullReferenceException(), data: properties));

            report = new HealthReport(entries, TimeSpan.FromMilliseconds(100));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task track_exceptions_by_default(bool saveDetailedReport, bool excludeHealthyReports)
        {
            // Arrange
            publisher = new ApplicationInsightsPublisher(string.Empty, saveDetailedReport, excludeHealthyReports);

            // Act
            await publisher.PublishAsync(report, cancellationToken);

            // Assert
            var itemsWithException = telemetryChannel.Items.Where(i => i is ExceptionTelemetry);
            Assert.Equal(2, itemsWithException.Count());

            foreach (var item in itemsWithException.Select(i => i as ExceptionTelemetry))
            {
                // Assert properties
                Assert.True(item.Properties.TryGetValue("AspNetCoreHealthCheckName", out string name));
                Assert.Equal(Environment.MachineName, item.Properties[nameof(Environment.MachineName)]);
                Assert.Equal(Assembly.GetEntryAssembly().GetName().Name, item.Properties[nameof(Assembly)]);

                // Assert metrics
                Assert.Equal(0, item.Metrics["AspNetCoreHealthCheckStatus"]);
                Assert.Equal(25, item.Metrics["AspNetCoreHealthCheckDuration"]);

                // Assert proper exception is reported based on name
                switch (name)
                {
                    case "unhealthy":
                        Assert.IsType<ArgumentNullException>(item.Exception);
                        break;

                    case "degraded_with_exception":
                        Assert.IsType<NullReferenceException>(item.Exception);
                        break;

                    default:
                        throw new Exception($"Unrecognized AspNetCoreHealthCheckName: {name}");
                }
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task dont_track_exceptions_if_parameter_is_false(bool saveDetailedReport, bool excludeHealthyReports)
        {
            // Arrange
            publisher = new ApplicationInsightsPublisher(string.Empty, saveDetailedReport, excludeHealthyReports, trackExceptions: false);

            // Act
            await publisher.PublishAsync(report, cancellationToken);

            // Assert
            var itemsWithException = telemetryChannel.Items.Where(i => i is ExceptionTelemetry);
            Assert.Empty(itemsWithException);
        }

        private class FakeTelemetryChannel : ITelemetryChannel
        {
            public List<ITelemetry> Items { get; set; } = new List<ITelemetry>();

            public void Send(ITelemetry item) => Items.Add(item);

            // These are not used for testing purposes, so they are left as not implemented methods.
            // The only one that is used is get DeveloperMode, which is set to a sane default.

            public bool? DeveloperMode { get => true; set => throw new NotImplementedException(); }
            public string EndpointAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public void Dispose() => throw new NotImplementedException();
            public void Flush() => throw new NotImplementedException();
        }
    }
}