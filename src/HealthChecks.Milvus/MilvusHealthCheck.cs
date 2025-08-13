using Microsoft.Extensions.Diagnostics.HealthChecks;
using Milvus.Client;

namespace HealthChecks.Milvus;

/// <summary>
/// A health check for Milvus services.
/// </summary>
public class MilvusHealthCheck : IHealthCheck
{
    private readonly MilvusClient _client;

    public MilvusHealthCheck(MilvusClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "db.system.name", "zilliz.milvus" },
            { "network.transport", "tcp" },
        };

        try
        {
            var result = await _client.HealthAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Empty response from Milvus client!");

            if (!result.IsHealthy)
            {
                checkDetails.Add("error.code", result.ErrorCode);
                checkDetails.Add("error.message", result.ErrorMsg);
                return new HealthCheckResult(context.Registration.FailureStatus, result.ToString(), null, data: checkDetails);
            }

            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
