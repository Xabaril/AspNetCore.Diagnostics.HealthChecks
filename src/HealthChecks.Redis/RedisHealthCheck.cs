using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Redis
{
    public class RedisHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connections = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        private readonly string _redisConnectionString;

        public RedisHealthCheck(string redisConnectionString)
        {
            _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_redisConnectionString, out ConnectionMultiplexer connection))
                {
                    connection = ConnectionMultiplexer.Connect(_redisConnectionString);

                    if (!_connections.TryAdd(_redisConnectionString, connection))
                    {
                        return Task.FromResult(
                             new HealthCheckResult(context.Registration.FailureStatus, description: "New redis connection can't be added into dictionary."));
                    }
                }

                connection.GetDatabase()
                    .Ping();

                return Task.FromResult(
                    HealthCheckResult.Healthy());
            }
            catch (Exception ex)
            {
                return Task.FromResult(
                    new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
            }
        }
    }
}
