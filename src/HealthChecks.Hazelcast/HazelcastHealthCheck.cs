using Microsoft.Extensions.Diagnostics.HealthChecks;
using Hazelcast;

namespace HealthChecks.Hazelcast;

public class HazelcastHealthCheck : IHealthCheck
{
    private readonly HazelcastHealthCheckOptions _options;

    public HazelcastHealthCheck(HazelcastHealthCheckOptions options)
    {
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthCheckResults = new List<HealthCheckResult>();

        foreach (var clusterName in _options.ClusterNames)
        {
            try
            {
                var options = new HazelcastOptionsBuilder()
                    .With(args =>
                    {
                        args.ClusterName = clusterName;
                        args.Networking.Addresses.Add($"{_options.Server}:{_options.Port}");
                    })
                    .Build();

                await using var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

                var map = await client.GetMapAsync<string, string>("healthcheck-map").ConfigureAwait(false);
                await map.SetAsync("healthcheck-key", "healthcheck-value").ConfigureAwait(false);
                var value = await map.GetAsync("healthcheck-key").ConfigureAwait(false);

                await map.DeleteAsync("healthcheck-key").ConfigureAwait(false);

                if (value == "healthcheck-value")
                {
                    healthCheckResults.Add(HealthCheckResult.Healthy($"Hazelcast cluster '{clusterName}' is healthy."));
                }
                else
                {
                    healthCheckResults.Add(HealthCheckResult.Unhealthy($"Hazelcast cluster '{clusterName}' health check failed."));
                }
            }
            catch (Exception ex)
            {
                healthCheckResults.Add(HealthCheckResult.Unhealthy($"Hazelcast cluster '{clusterName}' health check failed: {ex.Message}"));
            }
        }

        // Aggregate the results
        var unhealthyResults = healthCheckResults.FindAll(result => result.Status != HealthStatus.Healthy);

        if (unhealthyResults.Count > 0)
        {
            return HealthCheckResult.Unhealthy("One or more Hazelcast clusters are unhealthy.", data: new Dictionary<string, object>
            {
                { "UnhealthyClusters", unhealthyResults }
            });
        }
        else
        {
            return HealthCheckResult.Healthy("All Hazelcast clusters are healthy.");
        }
    }
}
