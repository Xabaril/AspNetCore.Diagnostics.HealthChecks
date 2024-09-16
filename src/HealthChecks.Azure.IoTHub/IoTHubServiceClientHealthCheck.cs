using Microsoft.Azure.Devices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.IoTHub;

public sealed class IoTHubServiceClientHealthCheck : IHealthCheck
{
    private readonly IotHubServiceClientOptions _options;
    private readonly ServiceClient _serviceClient;

    public IoTHubServiceClientHealthCheck(ServiceClient serviceClient, IotHubServiceClientOptions? options = default)
    {
        _options = options ?? new IotHubServiceClientOptions();
        _serviceClient = Guard.ThrowIfNull(serviceClient);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await ExecuteServiceConnectionCheckAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task ExecuteServiceConnectionCheckAsync(CancellationToken cancellationToken)
    {
        await _serviceClient.GetServiceStatisticsAsync(cancellationToken).ConfigureAwait(false);
    }
}
