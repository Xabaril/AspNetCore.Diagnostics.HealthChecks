using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;

namespace HealthChecks.SurrealDb;

/// <summary>
/// A health check for SurrealDb services.
/// </summary>
public class SurrealDbHealthCheck : IHealthCheck
{
    private readonly SurrealDbHealthCheckOptions _options;

    public SurrealDbHealthCheck(SurrealDbHealthCheckOptions options)
    {
        Guard.ThrowIfNull(options.ConnectionString, true);
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientOptions = new SurrealDbOptionsBuilder()
                .FromConnectionString(_options.ConnectionString)
                .Build();

            using var client = new SurrealDbClient(clientOptions);

            _options.Configure?.Invoke(client);
            await client.Connect(cancellationToken).ConfigureAwait(false);

            bool result = await client.Health(cancellationToken).ConfigureAwait(false);

            return _options.HealthCheckResultBuilder == null
                ? HealthCheckResult.Healthy()
                : _options.HealthCheckResultBuilder(result);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}