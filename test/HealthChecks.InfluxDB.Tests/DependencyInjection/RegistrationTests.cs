using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.InfluxDB.Tests.DependencyInjection
{
    public class ibmq_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services
                .AddHealthChecks()
                .AddInfluxDB("http://localhost:8086/?org=influxdata&bucket=dummy&latest=-72h", "ci_user", "password".ToCharArray(), "influxdb");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("influxdb");
            check.Should().BeOfType<InfluxDBHealthCheck>();
        }
    }
}
