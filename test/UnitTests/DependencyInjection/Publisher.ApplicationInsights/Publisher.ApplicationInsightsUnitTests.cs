using HealthChecks.Publisher.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Publisher.ApplicationInsights
{
    public class application_insights_publisher_registration_should
    {
        [Fact]
        public void add_healthcheck_when_properly_configured_with_instrumentation_key_parameter()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddApplicationInsightsPublisher("telemetrykey");

            var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }

        [Fact]
        public void add_healthcheck_when_application_insights_is_properly_configured_with_IOptions()
        {
            var services = new ServiceCollection();
            services.Configure<TelemetryConfiguration>(config => config.InstrumentationKey = "telemetrykey");

            services
                .AddHealthChecks()
                .AddApplicationInsightsPublisher();

            var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }
    }
}
