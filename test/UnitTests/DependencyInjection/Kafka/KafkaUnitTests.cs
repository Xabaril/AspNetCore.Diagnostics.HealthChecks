using Confluent.Kafka;
using HealthChecks.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Kafka
{
    public class kafka_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("kafka", typeof(KafkaHealthCheck), builder => builder.AddKafka(
                new ProducerConfig()));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-kafka-group", typeof(KafkaHealthCheck), builder => builder.AddKafka(
                new ProducerConfig(), name: "my-kafka-group"));
        }
    }
}
