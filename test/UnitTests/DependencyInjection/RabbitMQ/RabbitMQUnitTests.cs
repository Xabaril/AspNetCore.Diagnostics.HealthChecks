using HealthChecks.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.RabbitMQ
{
    public class rabbitmq_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("rabbitmq", typeof(RabbitMQHealthCheck), builder => builder.AddRabbitMQ("connectionstring"));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-rabbitmq", typeof(RabbitMQHealthCheck), builder => builder.AddRabbitMQ("connectionstring", name: "my-rabbitmq"));
        }
    }
}
