using HealthChecks.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Kafka
{
    public class kafka_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("kafka", typeof(KafkaHealthCheck), builder => builder.AddKafka(
                new Dictionary<string, object>()));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-kafka-group", typeof(KafkaHealthCheck), builder => builder.AddKafka(
                new Dictionary<string, object>(), name: "my-kafka-group"));
        }
    }
}
