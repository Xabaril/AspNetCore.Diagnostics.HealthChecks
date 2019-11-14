using FluentAssertions;
using HealthChecks.AzureIoTHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureIoTHub
{
    public class azure_iothub_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureIoTHub(setup => new AzureIoTHubOptions("https://iothub"));

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureiothub");
            check.GetType().Should().Be(typeof(AzureIoTHubHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureIoTHub(setup => new AzureIoTHubOptions("https://iothub"), name: "iothubcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("iothubcheck");
            check.GetType().Should().Be(typeof(AzureIoTHubHealthCheck));
        }

        [Fact]
        public void fail_when_invalid_connection_string_provided_in_configuration()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                    .AddAzureIoTHub(null);
            });
        }
    }
}
