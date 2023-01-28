using Azure.Identity;

namespace HealthChecks.AzureDigitalTwin.Tests
{
    public class azure_digital_twin_model_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinModels(
                    "MyDigitalTwinClientId",
                    "MyDigitalTwinClientSecret",
                    "TenantId",
                    "https://my-awesome-dt-host",
                    new string[] { "my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1" });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azuredigitaltwinmodels");
            check.ShouldBeOfType<AzureDigitalTwinModelsHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinModels(
                    "MyDigitalTwinClientId",
                    "MyDigitalTwinClientSecret",
                    "TenantId",
                    "https://my-awesome-dt-host",
                    new string[] { "my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1" },
                    name: "azuredigitaltwinmodels_check");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azuredigitaltwinmodels_check");
            check.ShouldBeOfType<AzureDigitalTwinModelsHealthCheck>();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinModels(string.Empty, string.Empty, string.Empty, string.Empty, new string[] { });

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
                .AddAzureDigitalTwinModels(
                    new MockTokenCredentials(),
                    "https://my-awesome-dt-host",
                    new string[] { "my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1" });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azuredigitaltwinmodels");
            check.ShouldBeOfType<AzureDigitalTwinModelsHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinModels(
                    new MockTokenCredentials(),
                    "https://my-awesome-dt-host",
                    new string[] { "my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1" },
                    name: "azuredigitaltwinmodels_check");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azuredigitaltwinmodels_check");
            check.ShouldBeOfType<AzureDigitalTwinModelsHealthCheck>();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided_by_credentials()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureDigitalTwinModels(new AzureCliCredential(), string.Empty, new string[] { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
