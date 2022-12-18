namespace HealthChecks.AzureServiceBus;

/// <summary>
/// Azure Service Bus configuration options.
/// </summary>
public class AzureServiceBusOptions
{
    /// <summary>
    /// Will use <c>PeekMessageAsync</c> method to determine status if set to <see langword="true"/> (default),
    /// otherwise; will use <c>GetProperties*</c> method.
    /// <remarks>
    /// Peek requires Listen claim to work. However, if only Sender claim using the Azure built-in roles (RBAC)
    /// <see href="https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#azure-service-bus-data-sender">Azure Service Bus Data Sender</see>
    /// is used set this to <see langword="false"/>. By default <see langword="true"/>.
    /// </remarks>
    /// </summary>
    public bool UsePeekMode { get; set; } = true;
}
