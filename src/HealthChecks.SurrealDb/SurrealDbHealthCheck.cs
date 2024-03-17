using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SurrealDb.Net;
using SurrealDb.Net.Models.Response;

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

            if (!string.IsNullOrWhiteSpace(_options.Query))
            {
                var response = await client.RawQuery(_options.Query, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (response.HasErrors)
                {
                    var error = response.Errors.First();
                    string errorMessage = error switch
                    {
                        SurrealDbErrorResult errorResult => errorResult.Details,
                        SurrealDbProtocolErrorResult protocolErrorResult => protocolErrorResult.Details ?? protocolErrorResult.Description,
                        _ => "Unknown error"
                    };

                    return _options.HealthCheckResultBuilder == null
                        ? HealthCheckResult.Unhealthy(errorMessage)
                        : _options.HealthCheckResultBuilder(false);
                }
            }

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
