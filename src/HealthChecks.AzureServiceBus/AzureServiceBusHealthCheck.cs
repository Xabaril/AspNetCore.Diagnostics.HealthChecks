using System.Collections.Concurrent;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace HealthChecks.AzureServiceBus
{
    public abstract class AzureServiceBusHealthCheck
    {
        protected static readonly ConcurrentDictionary<string, ServiceBusClient>
            ClientConnections = new();
        protected static readonly ConcurrentDictionary<string, ServiceBusAdministrationClient>
            ManagementClientConnections = new();
        protected static readonly ConcurrentDictionary<string, ServiceBusReceiver>
            ServiceBusReceivers = new();

        private string? ConnectionString { get; }

        protected string Prefix => ConnectionString ?? Endpoint!;

        private string? Endpoint { get; }

        private TokenCredential? TokenCredential { get; }

        protected AzureServiceBusHealthCheck(string connectionString)
        {
            ConnectionString = Guard.ThrowIfNull(connectionString, true);
        }

        protected AzureServiceBusHealthCheck(string endpoint, TokenCredential tokenCredential)
        {
            Endpoint = Guard.ThrowIfNull(endpoint, true);
            TokenCredential = Guard.ThrowIfNull(tokenCredential);
        }

        protected ServiceBusClient CreateClient()
        {
            return TokenCredential == null
                ? new ServiceBusClient(ConnectionString)
                : new ServiceBusClient(Endpoint, TokenCredential);
        }

        protected ServiceBusAdministrationClient CreateManagementClient()
        {
            return TokenCredential == null
                ? new ServiceBusAdministrationClient(ConnectionString)
                : new ServiceBusAdministrationClient(Endpoint, TokenCredential);
        }

        protected abstract string ConnectionKey { get; }
    }
}
