using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(string redisConnectionString, ILogger<RedisHealthCheck> logger = null)
        {
            _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(RedisHealthCheck)} is checking the Redis status.");

                using (var connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString))
                {
                    _logger?.LogInformation($"The {nameof(RedisHealthCheck)} check success.");

                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(RedisHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
