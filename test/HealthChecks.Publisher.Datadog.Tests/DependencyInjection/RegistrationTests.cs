using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace HealthChecks.Publisher.Datadog.Tests.DependencyInjection
{
    public class datadog_publisher_registration_should
    {
        [Fact]
        public void add_healthcheck_when_properly_configured()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddDatadogPublisher(serviceCheckName: "serviceCheckName", datadogAgentName: "127.0.0.1");

            var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }
    }
}
