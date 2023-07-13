namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Configuration options for <see cref="AzureServiceBusTopicHealthCheck"/>.
/// </summary>
public class AzureServiceBusTopicHealthCheckOptions : AzureServiceBusHealthCheckOptions
{
    /// <summary>
    /// The name of the topic to check.
    /// </summary>
    public string TopicName { get; set; }

    public AzureServiceBusTopicHealthCheckOptions(string topicName)
    {
        TopicName = topicName;
    }
}
