using FluentAssertions;
using HealthChecks.Azure.IoTHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Azure.IoTHub
{
    public class azure_iothub_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureIoTHub(options =>
                {
                    options.AddConnectionString("the-iot-connection-string");
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("iothub");
            check.GetType().Should().Be(typeof(IoTHubHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                 .AddAzureIoTHub(options =>
                 {
                     options.AddConnectionString("the-iot-connection-string");
                 },name:"iothubcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("iothubcheck");
            check.GetType().Should().Be(typeof(IoTHubHealthCheck));
        }
    }
}
