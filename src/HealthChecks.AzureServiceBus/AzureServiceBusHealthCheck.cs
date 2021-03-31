namespace HealthChecks.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using Azure.Core;
    using Azure.Messaging.ServiceBus.Administration;

    public abstract class AzureServiceBusHealthCheck
    {
        protected static readonly ConcurrentDictionary<string, ServiceBusAdministrationClient> ManagementClientConnections = new ConcurrentDictionary<string, ServiceBusAdministrationClient>()

        protected string ConnectionString { get; }
        protected string Endpoint { get; }
        protected TokenCredential TokenCredential { get; }

        protected AzureServiceBusHealthCheck (string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            ConnectionString = connectionString;
        }

        protected AzureServiceBusHealthCheck (string endpoint, TokenCredential tokenCredential)
        {
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

        protected string GetFirstPartOfKey()
        {
            return ConnectionString ?? Endpoint;
        }
    }
}
