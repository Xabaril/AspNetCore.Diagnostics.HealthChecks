namespace HealthChecks.AzureServiceBus.Tests
{
    public class azure_service_bus_queue_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue("cnn", "queueName");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azurequeue");
            check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue("cnn", "queueName",
                name: "azureservicebusqueuecheck");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azureservicebusqueuecheck");
            check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(string.Empty, string.Empty);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }

        [Fact]
        public void add_health_check_using_factories_when_properly_configured()
        {
            var services = new ServiceCollection();
            bool connectionStringFactoryCalled = false, queueNameFactoryCalled = false;
            services.AddHealthChecks()
                .AddAzureServiceBusQueue(_ =>
                    {
                        connectionStringFactoryCalled = true;
                        return "cnn";
                    },
                    _ =>
                    {
                        queueNameFactoryCalled = true;
                        return "queueName";
                    });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("azurequeue");
            check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
            connectionStringFactoryCalled.ShouldBeTrue();
            queueNameFactoryCalled.ShouldBeTrue();
        }
    }
}
