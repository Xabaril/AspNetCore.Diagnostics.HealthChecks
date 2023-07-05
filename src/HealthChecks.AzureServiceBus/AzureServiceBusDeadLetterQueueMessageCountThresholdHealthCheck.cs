using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusDeadLetterQueueMessageCountThresholdHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions>, IHealthCheck
{
    private readonly string _queueName;
    private readonly int _degradedThreshold;
    private readonly int _unhealthyThreshold;

    public AzureServiceBusDeadLetterQueueMessageCountThresholdHealthCheck(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions options)
        : base(options)
    {
        _queueName = Guard.ThrowIfNull(options.QueueName);
        _degradedThreshold = options.DegradedThreshold;
        _unhealthyThreshold = options.UnhealthyThreshold;
    }

    protected override string ConnectionKey => $"{Prefix}_{Options.QueueName}";

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ManagementClientConnections.TryGetValue(ConnectionKey, out var managementClient))
            {
                managementClient = CreateManagementClient();

                if (!ManagementClientConnections.TryAdd(ConnectionKey, managementClient))
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "No service bus administration client connection can't be added into dictionary.");
                }
            }

            var properties = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken).ConfigureAwait(false);
            if (properties.Value.DeadLetterMessageCount >= _unhealthyThreshold)
                return HealthCheckResult.Unhealthy($"Message in dead letter queue {_queueName} exceeded the amount of messages allowed for the unhealthy threshold {_unhealthyThreshold}/{properties.Value.DeadLetterMessageCount}");

            if (properties.Value.DeadLetterMessageCount >= _degradedThreshold)
                return HealthCheckResult.Degraded($"Message in dead letter queue {_queueName} exceeded the amount of messages allowed for the degraded threshold {_degradedThreshold}/{properties.Value.DeadLetterMessageCount}");

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
