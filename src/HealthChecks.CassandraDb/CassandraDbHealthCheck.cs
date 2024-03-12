using Cassandra;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.CassandraDb;

public class CassandraDbHealthCheck : IHealthCheck
{
    private readonly CassandraDbOptions _options;

    public CassandraDbHealthCheck(CassandraDbOptions options)
    {
        _options = Guard.ThrowIfNull(options);

        if (_options.ContactPoint is null && _options.Keyspace is null)
        {
            throw new ArgumentException("A connection or connection string must be set!", nameof(options));
        }
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = Cluster.Builder().AddContactPoint(_options.ContactPoint);
            _options.ConfigureClusterBuilder?.Invoke(builder);

            var cluster = builder.Build();

            ISession session = await cluster.ConnectAsync(_options.Keyspace).ConfigureAwait(false);


            RowSet rows = await session.ExecuteAsync(new SimpleStatement(_options.Query)).ConfigureAwait(false);
            var result = rows.FirstOrDefault();

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {

            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
