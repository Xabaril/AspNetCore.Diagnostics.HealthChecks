namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Configuration options for <see cref="AzureServiceBusQueueMessageCountThresholdHealthCheck"/>.
/// </summary>
public class AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions : AzureServiceBusQueueHealthCheckOptions
{
    /// <summary>
    /// Indicates if dead letter messages queue or active messages queue should be checked.
    /// </summary>
    public bool CheckDeadLetterMessages { get; }

    /// <summary>
    /// Threshold configuration for active messages queue.
    /// </summary>
    public AzureServiceBusQueueMessagesCountThreshold ActiveMessages { get; set; }

    /// <summary>
    /// Threshold configuration for dead letter messages queue.
    /// </summary>
    public AzureServiceBusQueueMessagesCountThreshold DeadLetterMessages { get; set; }

    public AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(string queueName, bool checkDeadLetterMessages, AzureServiceBusQueueMessagesCountThreshold threshold)
        : base(queueName)
    {
        CheckDeadLetterMessages = checkDeadLetterMessages;
        ActiveMessages = threshold;
    }
}
