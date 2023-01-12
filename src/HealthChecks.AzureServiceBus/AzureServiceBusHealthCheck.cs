using System.Collections.Concurrent;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;

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

        protected AzureServiceBusHealthCheck(AzureServiceBusOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                ConnectionString = options.ConnectionString;
                return;
            }

            if (!string.IsNullOrWhiteSpace(options.Endpoint))
            {
                TokenCredential = Guard.ThrowIfNull(options.Credential);
                Endpoint = options.Endpoint;
                return;
            }

            throw new ArgumentException("A connection string or endpoint must be set!", nameof(options));
        }

        protected ServiceBusClient CreateClient() =>
            TokenCredential == null
                ? new ServiceBusClient(ConnectionString)
                : new ServiceBusClient(Endpoint, TokenCredential);

        protected ServiceBusAdministrationClient CreateManagementClient() =>
            TokenCredential == null
                ? new ServiceBusAdministrationClient(ConnectionString)
                : new ServiceBusAdministrationClient(Endpoint, TokenCredential);

        protected abstract string ConnectionKey { get; }
    }
}
