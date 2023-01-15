using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus
{
    public abstract class AzureServiceBusHealthCheck
    {
        protected static readonly ConcurrentDictionary<string, ServiceBusClient> ClientConnections = new();

        protected static readonly ConcurrentDictionary<string, ServiceBusAdministrationClient>
            ManagementClientConnections = new();

        protected static readonly ConcurrentDictionary<string, ServiceBusReceiver> ServiceBusReceivers = new();

        private readonly AzureServiceBusHealthCheckOptions _options;

        protected string Prefix => _options.ConnectionString ?? _options.FullyQualifiedNamespace!;

        protected abstract string ConnectionKey { get; }

        protected AzureServiceBusHealthCheck(AzureServiceBusHealthCheckOptions options)
        {
            _options = options;

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
                return;

            if (!string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace))
            {
                Guard.ThrowIfNull(options.Credential);
                return;
            }

            throw new ArgumentException("A connection string or endpoint must be set!", nameof(options));
        }

        protected ServiceBusClient CreateClient() =>
            _options.Credential is null
                ? new ServiceBusClient(_options.ConnectionString)
                : new ServiceBusClient(_options.FullyQualifiedNamespace, _options.Credential);

        protected ServiceBusAdministrationClient CreateManagementClient() =>
            _options.Credential is null
                ? new ServiceBusAdministrationClient(_options.ConnectionString)
                : new ServiceBusAdministrationClient(_options.FullyQualifiedNamespace, _options.Credential);
    }
}
