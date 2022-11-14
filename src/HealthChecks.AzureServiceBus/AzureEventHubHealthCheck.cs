using System.Collections.Concurrent;
using Azure.Core;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureServiceBus
{
    public class AzureEventHubHealthCheck : IHealthCheck
    {
        private const string ENTITY_PATH_SEGMENT = "EntityPath=";

        private static readonly ConcurrentDictionary<string, EventHubProducerClient> _clientConnections = new();

        private readonly string? _connectionString;
        private readonly string? _endpoint;
        private readonly string? _eventHubName;
        private readonly TokenCredential? _tokenCredential;

        private string? _connectionKey;

        private string ConnectionKey =>
            _connectionKey ??= _connectionString ?? $"{_endpoint}_{_eventHubName}";

        public AzureEventHubHealthCheck(string connectionString, string eventHubName)
        {
            Guard.ThrowIfNull(connectionString, true);
            Guard.ThrowIfNull(eventHubName, true);

            _connectionString = connectionString.Contains(ENTITY_PATH_SEGMENT) ? connectionString : $"{connectionString};{ENTITY_PATH_SEGMENT}{eventHubName}";

            if (!_clientConnections.ContainsKey(_connectionString))
            {
                _clientConnections.TryAdd(_connectionString, new EventHubProducerClient(_connectionString));
            }
        }

        public AzureEventHubHealthCheck(EventHubConnection connection)
        {
            Guard.ThrowIfNull(connection);

            _connectionString = $"{connection.FullyQualifiedNamespace};{ENTITY_PATH_SEGMENT}{connection.EventHubName}";

            if (!_clientConnections.ContainsKey(_connectionString))
            {
                _clientConnections.TryAdd(_connectionString, new EventHubProducerClient(connection));
            }
        }

        public AzureEventHubHealthCheck(string endpoint, string eventHubName, TokenCredential tokenCredential)
        {
            _endpoint = Guard.ThrowIfNull(endpoint, true);
            _eventHubName = Guard.ThrowIfNull(eventHubName, true);
            _tokenCredential = Guard.ThrowIfNull(tokenCredential);

            string key = ConnectionKey;
            if (!_clientConnections.ContainsKey(key))
            {
                _clientConnections.TryAdd(key, new EventHubProducerClient(_endpoint, _eventHubName, _tokenCredential));
            }
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _clientConnections.GetOrAdd(ConnectionKey, _ => CreateClient());
                _ = await client.GetEventHubPropertiesAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private EventHubProducerClient CreateClient() => _tokenCredential == null
            ? new EventHubProducerClient(_connectionString)
            : new EventHubProducerClient(_endpoint, _eventHubName, _tokenCredential);
    }
}
