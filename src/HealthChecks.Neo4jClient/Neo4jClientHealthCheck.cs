using Microsoft.Extensions.Diagnostics.HealthChecks;
using Neo4jClient;

namespace HealthChecks.Neo4jClient;

/// <summary>
/// A health check for Neo4j databases.
/// </summary>
public class Neo4jClientHealthCheck : IHealthCheck
{
    private readonly Neo4jClientHealthCheckOptions _options;

    /// <summary>
    /// Creates an instance with the options passed to it
    /// </summary>
    public Neo4jClientHealthCheck(Neo4jClientHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    ///<inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _options.GraphClient ??= new BoltGraphClient(new Uri(_options.Host),
                                                         _options.Username,
                                                         _options.Password,
                                                         _options.Realm,
                                                         _options.EncryptionLevel,
                                                         _options.SerializeNullValues,
                                                         _options.UseDriverDataTypes);


            var graphClient = _options.GraphClient;

            await graphClient.ConnectAsync().ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}
