using HealthChecks.RabbitMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "rabbitmq";

        public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, string rabbitMQConnectionString)
        {
            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new RabbitMQHealthCheck(rabbitMQConnectionString, sp.GetService<ILogger<RabbitMQHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
