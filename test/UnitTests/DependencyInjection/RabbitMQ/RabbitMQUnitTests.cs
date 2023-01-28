using HealthChecks.RabbitMQ;

namespace UnitTests.HealthChecks.DependencyInjection.RabbitMQ
{
    public class rabbitmq_registration_should
    {
        private readonly string _fakeConnectionString = "amqp://server";
        private readonly string _defaultCheckName = "rabbitmq";

        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddRabbitMQ(rabbitConnectionString: _fakeConnectionString);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(_defaultCheckName);
            check.ShouldBeOfType<RabbitMQHealthCheck>();
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            var customCheckName = "my-" + _defaultCheckName;

            services.AddHealthChecks()
                .AddRabbitMQ(_fakeConnectionString, name: customCheckName);

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(customCheckName);
            check.ShouldBeOfType<RabbitMQHealthCheck>();
        }
    }
}
