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

    /// <summary>
    /// Will use <c>CreateMessageBatchAsync</c> method to determine status if set to <see langword="true"/> (default),
    /// otherwise; will use <c>GetProperties*</c> method.
    /// </summary>
    /// <remarks>
    /// CreateMessageBatch requires Send claim to work. However, if only Receiver claim using the Azure built-in roles (RBAC)
    /// <see href="https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/integration#azure-service-bus-data-receiver">Azure Service Bus Data Receiver</see>
    /// is used set this to <see langword="false"/>. By default <see langword="true"/>.
    /// </remarks>
    public bool UseCreateMessageBatchAsyncMode { get; set; } = true;

    public AzureServiceBusTopicHealthCheckOptions(string topicName)
    {
        TopicName = topicName;
    }
}
