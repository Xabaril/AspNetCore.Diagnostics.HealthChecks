using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Redis
{
    public class RedisHealthCheck
        : IHealthCheck
    {
        private readonly string _redisConnectionString;
        public RedisHealthCheck(string redisConnectionString)
        {
            _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString))
                {
                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
