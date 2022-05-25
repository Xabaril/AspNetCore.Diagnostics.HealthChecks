using Azure.Identity;

namespace HealthChecks.AzureDigitalTwin.Tests
{
    public class azure_digital_twin_instance_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinInstance(
                    "MyDigitalTwinClientId",
                    "MyDigitalTwinClientSecret",
                    "TenantId",
                    "https://my-awesome-dt-host",
                    "my_dt_instance_name");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwininstance");
            check.GetType().Should().Be(typeof(AzureDigitalTwinInstanceHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinInstance(
                    "MyDigitalTwinClientId",
                    "MyDigitalTwinClientSecret",
                    "TenantId",
                    "https://my-awesome-dt-host",
                    "my_dt_instance_name",
                    name: "azuredigitaltwininstance_check");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwininstance_check");
            check.GetType().Should().Be(typeof(AzureDigitalTwinInstanceHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinInstance(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }

        [Fact]
        public void add_health_check_when_properly_configured_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinInstance(
                    new MockTokenCredentials(),
                    "https://my-awesome-dt-host",
                    "my_dt_instance_name");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwininstance");
            check.GetType().Should().Be(typeof(AzureDigitalTwinInstanceHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinInstance(
                    new MockTokenCredentials(),
                    "https://my-awesome-dt-host",
                    "my_dt_instance_name",
                    name: "azuredigitaltwininstance_check");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuredigitaltwininstance_check");
            check.GetType().Should().Be(typeof(AzureDigitalTwinInstanceHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinInstance(new AzureCliCredential(), string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
