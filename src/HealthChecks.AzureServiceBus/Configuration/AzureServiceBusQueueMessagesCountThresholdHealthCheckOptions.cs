namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Configuration options for <see cref="AzureServiceBusQueueMessageCountThresholdHealthCheck"/>.
/// </summary>
public class AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions : AzureServiceBusQueueHealthCheckOptions
{
    /// <summary>
    /// Threshold configuration for active messages queue.
    /// </summary>
    public AzureServiceBusQueueMessagesCountThreshold? ActiveMessages { get; set; }

    /// <summary>
    /// Threshold configuration for dead letter messages queue.
    /// </summary>
    public AzureServiceBusQueueMessagesCountThreshold? DeadLetterMessages { get; set; }

    public AzureServiceBusQueueMessagesCountThresholdHealthCheckOptions(string queueName)
        : base(queueName)
    {
    }
}
