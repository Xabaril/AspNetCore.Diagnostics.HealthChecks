using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace HealthChecks.Redis
{
    public class RedisHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connections = new();
        private readonly string _redisConnectionString;

        public RedisHealthCheck(string redisConnectionString)
        {
            _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_redisConnectionString, out var connection))
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(_redisConnectionString);

                    if (!_connections.TryAdd(_redisConnectionString, connection))
                    {
                        // Dispose new connection which we just created, because we don't need it.
                        connection.Dispose();
                        connection = _connections[_redisConnectionString];
                    }
                }

                foreach (var endPoint in connection.GetEndPoints(configuredOnly: true))
                {
                    var server = connection.GetServer(endPoint);

                    if (server.ServerType != ServerType.Cluster)
                    {
                        await connection.GetDatabase().PingAsync();
                        await server.PingAsync();
                    }
                    else
                    {
                        var clusterInfo = await server.ExecuteAsync("CLUSTER", "INFO");

                        if (clusterInfo is object && !clusterInfo.IsNull)
                        {
                            if (!clusterInfo.ToString()!.Contains("cluster_state:ok"))
                            {
                                //cluster info is not ok!
                                return new HealthCheckResult(context.Registration.FailureStatus, description: $"INFO CLUSTER is not on OK state for endpoint {endPoint}");
                            }
                        }
                        else
                        {
                            //cluster info cannot be read for this cluster node
                            return new HealthCheckResult(context.Registration.FailureStatus, description: $"INFO CLUSTER is null or can't be read for endpoint {endPoint}");
                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                _connections.TryRemove(_redisConnectionString, out var connection);
                connection?.Dispose();
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
