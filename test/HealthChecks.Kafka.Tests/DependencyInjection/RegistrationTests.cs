using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.Kafka.Tests.DependencyInjection
{

    public class kafka_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddKafka(new ProducerConfig());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("kafka");
            check.GetType().Should().Be(typeof(KafkaHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddKafka(new ProducerConfig(), name: "my-kafka-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-kafka-group");
            check.GetType().Should().Be(typeof(KafkaHealthCheck));
        }
        [Fact]
        public void add_health_check_with_log_handling_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddKafka(new ProducerConfig(), message => {});

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("kafka");
            check.GetType().Should().Be(typeof(KafkaHealthCheck));
        }
    }
}
