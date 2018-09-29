using HealthChecks.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "kafka";

        public static IHealthChecksBuilder AddKafka(this IHealthChecksBuilder builder, Dictionary<string, object> config)
        {
            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new KafkaHealthCheck(config, sp.GetService<ILogger<KafkaHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
