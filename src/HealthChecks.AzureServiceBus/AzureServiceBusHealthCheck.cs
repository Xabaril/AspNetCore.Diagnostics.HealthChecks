using System;
using System.Collections.Concurrent;
using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;

namespace HealthChecks.AzureServiceBus
{
    public abstract class AzureServiceBusHealthCheck
    {
        protected static readonly ConcurrentDictionary<string, ServiceBusAdministrationClient>
            ManagementClientConnections = new();

        private string ConnectionString { get; }

        protected string Prefix => ConnectionString ?? Endpoint;

        private string Endpoint { get; }

        private TokenCredential TokenCredential { get; }

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

            if (tokenCredential == null)
            {
                throw new ArgumentNullException(nameof(tokenCredential));
            }

            Endpoint = endpoint;
            TokenCredential = tokenCredential;
        }


        protected ServiceBusAdministrationClient CreateManagementClient()
        {
            ServiceBusAdministrationClient managementClient;
            if (TokenCredential != null)
            {
                managementClient = new ServiceBusAdministrationClient(Endpoint, TokenCredential);
            }
            else
            {
                managementClient = new ServiceBusAdministrationClient(ConnectionString);
            }

            return managementClient;
        }

        protected abstract string ConnectionKey { get; }
    }
}
