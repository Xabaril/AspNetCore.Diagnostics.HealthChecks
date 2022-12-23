using HealthChecks.EventStore;

namespace HealthChecks.Consul.Tests.DependencyInjection
{
    public class eventstore_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddEventStore("connection-string");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("eventstore");
            check.ShouldBeOfType<EventStoreHealthCheck>();
        }

        [Fact]
        public void add_health_check_when_properly_configured_using_service_provider_overload()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddEventStore(sp => "connection-string");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("eventstore");
            check.ShouldBeOfType<EventStoreHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddEventStore("connection-string", name: "my-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe("my-group");
            check.ShouldBeOfType<EventStoreHealthCheck>();
        }
    }
}
