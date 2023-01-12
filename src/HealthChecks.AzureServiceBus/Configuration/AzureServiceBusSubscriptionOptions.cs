namespace HealthChecks.AzureServiceBus.Configuration;

/// <summary>
/// Azure Service Bus Queue configuration options.
/// </summary>
public class AzureServiceBusSubscriptionOptions : AzureServiceBusTopicOptions
{
    /// <summary>
    /// The subscription name of the topic subscription to check.
    /// </summary>
    public string SubscriptionName { get; set; }

    /// <summary>
    /// Will use <c>PeekMessageAsync</c> method to determine status if set to <see langword="true"/> (default),
    /// otherwise; will use <c>GetProperties*</c> method.
    /// </summary>
    /// <remarks>
    /// Peek requires Listen claim to work. However, if only Sender claim using the Azure built-in roles (RBAC)
    /// <see href="https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#azure-service-bus-data-sender">Azure Service Bus Data Sender</see>
    /// is used set this to <see langword="false"/>. By default <see langword="true"/>.
    /// </remarks>
    public bool UsePeekMode { get; set; } = true;

    public AzureServiceBusSubscriptionOptions(string topicName, string subscriptionName)
        : base(topicName)
    {
        SubscriptionName = subscriptionName;
    }
}
