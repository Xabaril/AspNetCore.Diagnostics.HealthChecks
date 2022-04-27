using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Xunit;

namespace HealthChecks.AzureDigitalTwin.Tests
{
    public class azure_digital_twin_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwin");
            check.GetType().Should().Be(typeof(AzureDigitalTwinSubscriptionHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwin("MyDigitalTwinClientId", "MyDigitalTwinClientSecret", "TenantId", name: "azuredigitaltwincheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwincheck");
            check.GetType().Should().Be(typeof(AzureDigitalTwinSubscriptionHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwin(string.Empty, string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }

        [Fact]
        public void add_health_check_when_properly_configured_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwin(credentials: new MockServiceClientCredentials());

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwin");
            check.GetType().Should().Be(typeof(AzureDigitalTwinSubscriptionHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwin(new MockServiceClientCredentials(), name: "azuredigitaltwincheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwincheck");
            check.GetType().Should().Be(typeof(AzureDigitalTwinSubscriptionHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwin(default(ServiceClientCredentials));

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
