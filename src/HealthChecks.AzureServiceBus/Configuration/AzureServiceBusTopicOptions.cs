namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Azure Service Bus Topic configuration options.
/// </summary>
public class AzureServiceBusTopicOptions : AzureServiceBusOptions
{
    /// <summary>
    /// The topic name of the topic to check.
    /// </summary>
    public string TopicName { get; set; }

    public AzureServiceBusTopicOptions(string topicName)
    {
        TopicName = topicName;
    }
}
