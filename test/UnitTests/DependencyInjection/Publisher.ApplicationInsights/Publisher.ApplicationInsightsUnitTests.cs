using HealthChecks.Publisher.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace UnitTests.DependencyInjection.Publisher.ApplicationInsights
{
    public class application_insights_publisher_registration_should
    {
        [Fact]
        public void add_publisher_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddApplicationInsightsPublisher();

            var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }
    }
}
