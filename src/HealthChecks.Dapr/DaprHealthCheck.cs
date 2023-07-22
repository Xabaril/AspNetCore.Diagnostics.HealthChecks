using Dapr.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Dapr;

public class DaprHealthCheck : IHealthCheck
{
    private readonly DaprClient _daprClient;

    public DaprHealthCheck(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return await _daprClient.CheckHealthAsync(cancellationToken).ConfigureAwait(false)
            ? new HealthCheckResult(HealthStatus.Healthy)
            : new HealthCheckResult(context.Registration.FailureStatus);
    }
}
