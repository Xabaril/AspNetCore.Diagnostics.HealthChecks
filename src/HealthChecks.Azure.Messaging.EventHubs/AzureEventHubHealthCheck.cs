using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.Messaging.EventHubs;

public sealed class AzureEventHubHealthCheck : IHealthCheck
{
    private readonly EventHubProducerClient _client;

    public AzureEventHubHealthCheck(EventHubProducerClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "messaging.system", "eventhubs" }
        };

        try
        {
            _ = await _client.GetEventHubPropertiesAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }
}
