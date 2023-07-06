using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusQueueMessageCountThresholdHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions>, IHealthCheck
{
    private readonly string _queueName;
    private readonly AzureServiceBusQueueMessagesCountThreshold _activeMessagesThreshold;
    private readonly AzureServiceBusQueueMessagesCountThreshold _deadLetterMessagesThreshold;

    public AzureServiceBusQueueMessageCountThresholdHealthCheck(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions options)
        : base(options)
    {
        _queueName = Guard.ThrowIfNull(options.QueueName);
        _activeMessagesThreshold = Guard.ThrowIfNull(options.ActiveMessages);
        _deadLetterMessagesThreshold = Guard.ThrowIfNull(options.DeadLetterMessages);
    }

    protected override string ConnectionKey => $"{Prefix}_{_queueName}";

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, CreateManagementClient());

            var properties = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken).ConfigureAwait(false);

            var activeQueueHealthStatus = CheckHealthStatus(
                properties.ActiveMessageCount,
                _activeMessagesThreshold,
                "queue");

            if (activeQueueHealthStatus.Status != HealthStatus.Healthy)
            {
                return activeQueueHealthStatus;
            }

            var deadLetterQueueHealthStatus = CheckHealthStatus(
                properties.DeadLetterMessageCount,
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
        AzureServiceBusQueueMessagesCountThreshold threshold,
        string queueType)
    {
        if (threshold is null)
        {
            return HealthCheckResult.Healthy();
        }

        if (messagesCount >= threshold.UnhealthyThreshold)
        {
            return HealthCheckResult.Unhealthy($"Message in {queueType} {_queueName} exceeded the amount of messages allowed for the unhealthy threshold {threshold.UnhealthyThreshold}/{messagesCount}");
        }

        if (messagesCount >= threshold.DegradedThreshold)
        {
            return HealthCheckResult.Degraded($"Message in {queueType} {_queueName} exceeded the amount of messages allowed for the degraded threshold {threshold.DegradedThreshold}/{messagesCount}");
        }

        return HealthCheckResult.Healthy();
    }
}
