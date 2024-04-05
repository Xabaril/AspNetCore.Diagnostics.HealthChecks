using Microsoft.Extensions.Diagnostics.HealthChecks;
using Qdrant.Client;

namespace HealthChecks.Qdrant;

public class QdrantHealthCheck : IHealthCheck
{
    private readonly QdrantHealthCheckOptions _options;
    private QdrantClient? _connection;

    public QdrantHealthCheck(QdrantHealthCheckOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _connection = options.Client;

        if (_connection is null && _options.ConnectionUri is null)
        {
            throw new ArgumentException("A connection string must be set!", nameof(options));
        }
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await EnsureConnection().ListAliasesAsync(cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private QdrantClient EnsureConnection()
    {
        if (_connection is null)
        {
            if (_options.ConnectionUri is null)
            {
                throw new ArgumentException("A connection string must be set!", nameof(_options));
            }

            _connection = new QdrantClient(address: _options.ConnectionUri,
                apiKey: _options.ApiKey,
                grpcTimeout: _options.RequestedConnectionTimeout);
        }

        return _connection;
    }
}
