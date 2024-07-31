using Microsoft.Extensions.Diagnostics.HealthChecks;
using Milvus.Client;

namespace HealthChecks.Milvus;

/// <summary>
/// A health check for Milvus services.
/// </summary>
public class MilvusHealthCheck : IHealthCheck
{
    //private readonly MilvusHealthCheckOptions _options;
    private readonly MilvusClient _client;

    public MilvusHealthCheck(MilvusClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _client.HealthAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Empty response from Milvus client!");

            if (!result.IsHealthy)
            {
                return new HealthCheckResult(context.Registration.FailureStatus,
                    result.ToString(),
                    null,
                    new Dictionary<string, object>
                    {
                        { "responseErrorCode", result.ErrorCode },
                        { "responseErrorMsg", result.ErrorMsg }
                    });
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
