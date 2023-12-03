using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusQueueMessageCountThresholdHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions>, IHealthCheck
{
    private readonly string _queueName;
    private readonly AzureServiceBusQueueMessagesCountThreshold? _activeMessagesThreshold;
    private readonly AzureServiceBusQueueMessagesCountThreshold? _deadLetterMessagesThreshold;

    public AzureServiceBusQueueMessageCountThresholdHealthCheck(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions options, ServiceBusClientProvider clientProvider)
        : base(options, clientProvider)
    {
        _queueName = Guard.ThrowIfNull(options.QueueName);
        _activeMessagesThreshold = options.ActiveMessages;
        _deadLetterMessagesThreshold = options.DeadLetterMessages;
    }

    public AzureServiceBusQueueMessageCountThresholdHealthCheck(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions options)
        : this(options, new ServiceBusClientProvider())
    { }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var managementClient = ClientCache.GetOrAdd(ConnectionKey, _ => CreateManagementClient());

            var properties = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken).ConfigureAwait(false);

            var activeQueueHealthStatus = CheckHealthStatus(
                properties.Value.ActiveMessageCount,
                _activeMessagesThreshold,
                "queue");

            if (activeQueueHealthStatus.Status != HealthStatus.Healthy)
            {
                return activeQueueHealthStatus;
            }

            var deadLetterQueueHealthStatus = CheckHealthStatus(
                properties.Value.DeadLetterMessageCount,
                _deadLetterMessagesThreshold,
                "dead letter queue");

            if (deadLetterQueueHealthStatus.Status != HealthStatus.Healthy)
            {
                return deadLetterQueueHealthStatus;
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private HealthCheckResult CheckHealthStatus(
        long messagesCount,
        AzureServiceBusQueueMessagesCountThreshold? threshold,
        string queueType)
    {
        if (threshold is null)
        {
            return HealthCheckResult.Healthy();
        }

        if (messagesCount >= threshold.Value.UnhealthyThreshold)
        {
            return HealthCheckResult.Unhealthy($"Message in {queueType} {_queueName} exceeded the amount of messages allowed for the unhealthy threshold {threshold.Value.UnhealthyThreshold}/{messagesCount}");
        }

        if (messagesCount >= threshold.Value.DegradedThreshold)
        {
            return HealthCheckResult.Degraded($"Message in {queueType} {_queueName} exceeded the amount of messages allowed for the degraded threshold {threshold.Value.DegradedThreshold}/{messagesCount}");
        }

        return HealthCheckResult.Healthy();
    }
}
