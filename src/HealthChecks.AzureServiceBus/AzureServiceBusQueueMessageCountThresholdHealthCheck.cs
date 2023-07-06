using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus;

public class AzureServiceBusQueueMessageCountThresholdHealthCheck : AzureServiceBusHealthCheck<AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions>, IHealthCheck
{
    private readonly bool _checkDeadLetterMessages;
    private readonly string _queueName;
    private readonly int _degradedThreshold;
    private readonly int _unhealthyThreshold;

    public AzureServiceBusQueueMessageCountThresholdHealthCheck(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions options)
        : base(options)
    {
        _checkDeadLetterMessages = options.CheckDeadLetterMessages;
        _queueName = Guard.ThrowIfNull(options.QueueName);

        var threshold = GetThreshold(options);

        _degradedThreshold = threshold.DegradedThreshold;
        _unhealthyThreshold = threshold.UnhealthyThreshold;
    }

    protected override string ConnectionKey => $"{Prefix}_{_queueName}";

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var managementClient = ManagementClientConnections.GetOrAdd(ConnectionKey, CreateManagementClient());

            var properties = await managementClient.GetQueueRuntimePropertiesAsync(_queueName, cancellationToken).ConfigureAwait(false);

            var messagesCount = GetMessagesCount(properties);

            if (messagesCount >= _unhealthyThreshold)
            {
                return HealthCheckResult.Unhealthy($"Message in {GetQueueType()} {_queueName} exceeded the amount of messages allowed for the unhealthy threshold {_unhealthyThreshold}/{messagesCount}");
            }

            if (messagesCount >= _degradedThreshold)
            {
                return HealthCheckResult.Degraded($"Message in {GetQueueType()} {_queueName} exceeded the amount of messages allowed for the degraded threshold {_degradedThreshold}/{messagesCount}");
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private long GetMessagesCount(QueueRuntimeProperties queueRuntimeProperty) => _checkDeadLetterMessages
        ? queueRuntimeProperty.DeadLetterMessageCount
        : queueRuntimeProperty.ActiveMessageCount;

    private string GetQueueType() => _checkDeadLetterMessages
        ? "dead letter queue"
        : "queue";

    private static AzureServiceBusQueueMessagesCountThreshold GetThreshold(AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions options) =>
       options.CheckDeadLetterMessages
            ? options.DeadLetterMessages
            : options.ActiveMessages;
}
