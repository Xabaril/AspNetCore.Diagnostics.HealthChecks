using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace HealthChecks.Publisher.ApplicationInsights.Tests.DependencyInjection
{
    public class prometheus_publisher_registration_should
    {
        [Fact]
        [System.Obsolete]
        public void add_healthcheck_when_properly_configured()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddPrometheusGatewayPublisher("http://endpoint.com", "job_name");

            using var serviceProvider = services.BuildServiceProvider();
            var publisher = serviceProvider.GetService<IHealthCheckPublisher>();

            Assert.NotNull(publisher);
        }
    }
}
