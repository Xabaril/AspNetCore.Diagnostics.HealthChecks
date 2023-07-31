using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace HealthChecks.AzureServiceBus;

public class ServiceBusClientProvider
{
    public virtual ServiceBusClient CreateClient(string? connectionString)
        => new ServiceBusClient(connectionString);

    public virtual ServiceBusClient CreateClient(string? fullyQualifiedName, TokenCredential credential)
        => new ServiceBusClient(fullyQualifiedName, credential);

    public virtual ServiceBusAdministrationClient CreateManagementClient(string? connectionString)
        => new ServiceBusAdministrationClient(connectionString);

    public virtual ServiceBusAdministrationClient CreateManagementClient(string? fullyQualifiedName, TokenCredential credential)
        => new ServiceBusAdministrationClient(fullyQualifiedName, credential);
}
