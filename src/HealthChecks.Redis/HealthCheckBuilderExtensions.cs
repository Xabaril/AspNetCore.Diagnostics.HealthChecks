using HealthChecks.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "redis";
        public static IHealthChecksBuilder AddRedis(this IHealthChecksBuilder builder, string redisConnectionString)
        {
            return builder.Add(new HealthCheckRegistration(
               NAME,
               sp => new RedisHealthCheck(redisConnectionString, sp.GetService<ILogger<RedisHealthCheck>>()),
               null,
               new string[] { NAME }));
        }
    }
}
