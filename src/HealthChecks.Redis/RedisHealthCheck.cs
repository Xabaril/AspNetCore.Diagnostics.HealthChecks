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

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_redisConnectionString, out ConnectionMultiplexer connection))
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);

                    if (!_connections.TryAdd(_redisConnectionString, connection))
                    {
                        // Dispose new connection which we just created, because we don't need it.
                        connection.Dispose();
                        connection = _connections[_redisConnectionString];
                    }
                }

                await connection.GetDatabase()
                    .PingAsync();

				return HealthCheckResult.Healthy();
			}
            catch (Exception ex)
            {
				return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
			}
        }
    }
}
