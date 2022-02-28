using System.Collections.Concurrent;
using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;

namespace HealthChecks.AzureServiceBus
{
    public abstract class AzureServiceBusHealthCheck
    {
        protected static readonly ConcurrentDictionary<string, ServiceBusAdministrationClient>
            ManagementClientConnections = new();

        private string? ConnectionString { get; }

        protected string Prefix => ConnectionString ?? Endpoint!;

        private string? Endpoint { get; }

        private TokenCredential? TokenCredential { get; }

        protected AzureServiceBusHealthCheck(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            ConnectionString = connectionString;
        }

        protected AzureServiceBusHealthCheck(string endpoint, TokenCredential tokenCredential)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            Endpoint = endpoint;
            TokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
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
