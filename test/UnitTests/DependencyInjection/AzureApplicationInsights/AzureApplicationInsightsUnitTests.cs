using FluentAssertions;
using HealthChecks.AzureApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using Xunit;

namespace UnitTests.DependencyInjection.AzureApplicationInsights
{
    public class azure_application_insights_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureApplicationInsights("instrumentationKey");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureappinsights");
            check.GetType().Should().Be(typeof(AzureApplicationInsightsHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var healthCheckName = "azureappinsightscheck";
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureApplicationInsights("instrumentationKey", name: healthCheckName);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be(healthCheckName);
            check.GetType().Should().Be(typeof(AzureApplicationInsightsHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureApplicationInsights(string.Empty);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
