using Hazelcast;
using Hazelcast.Networking;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Hazelcast;

/// <summary>
/// A health check for Hazelcast services.
/// </summary>
public class HazelcastHealthCheck : IHealthCheck
{
    private readonly HazelcastHealthCheckOptions _options;

    public HazelcastHealthCheck(HazelcastHealthCheckOptions options)
    {
        _ = Guard.ThrowIfNull(options);
        _ = Guard.ThrowIfNull(options.ConnectionHost, true);

        _options = options;
        if (string.IsNullOrEmpty(_options.ClientName))
        {
            _options.ClientName = $"HazelcastHealthcheck_{Environment.MachineName}";
        }
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options =>
            {
                options.Networking.Addresses.Add(_options.ConnectionHost);
                options.Networking.ReconnectMode = ReconnectMode.DoNotReconnect;
                options.Networking.ConnectionTimeoutMilliseconds = (int)_options.ConnectionTimeout.TotalMilliseconds;

                options.ClusterName = _options.ClusterName;
                options.ClientName = _options.ClientName;
            }, cancellationToken).ConfigureAwait(false);

            return client.IsActive && client.IsConnected
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus, description: $"Hazelcast client connection failed: {client.State}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
