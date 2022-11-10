namespace HealthChecks.EventStore.Tests.DependencyInjection
{
    public class eventstore_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddEventStore("ConnectTo=tcp://localhost:1113; UseSslConnection=false; HeartBeatTimeout=500");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("eventstore");
            check.GetType().Should().Be(typeof(EventStoreHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddEventStore("ConnectTo=tcp://localhost:1113; UseSslConnection=false; HeartBeatTimeout=500", name: "my-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-group");
            check.GetType().Should().Be(typeof(EventStoreHealthCheck));
        }
    }
}
