using System.Collections.ObjectModel;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.Messaging.EventHubs;

public sealed class AzureEventHubHealthCheck : IHealthCheck
{
    private readonly EventHubProducerClient _client;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                { "health_check.task", "ready" },
                { "messaging.system", "eventhubs" }
    };

    public AzureEventHubHealthCheck(EventHubProducerClient client)
    {
        _client = Guard.ThrowIfNull(client);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            _ = await _client.GetEventHubPropertiesAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
