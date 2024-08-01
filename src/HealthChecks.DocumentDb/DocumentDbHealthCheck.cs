using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DocumentDb;

public class DocumentDbHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, DocumentClient> _connections = new();
    private readonly DocumentDbOptions _options;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "healthcheck.name", nameof(DocumentDbHealthCheck) },
                    { "healthcheck.task", "ready" },
                    { "db.system", "documentdb" },
                    { "event.name", "database.healthcheck"},
                    { "client.address", Dns.GetHostName()},
                    { "network.protocol.name", "http" },
                    { "network.transport", "tcp" }
    };

    public DocumentDbHealthCheck(DocumentDbOptions documentDbOptions)
    {
        Guard.ThrowIfNull(documentDbOptions.UriEndpoint);
        Guard.ThrowIfNull(documentDbOptions.PrimaryKey);

        _options = documentDbOptions;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            checkDetails.Add("db.namespace", _options.DatabaseName ?? "");
            checkDetails.Add("db.collection.name", _options.CollectionName ?? "");
            if (!_connections.TryGetValue(_options.UriEndpoint, out var documentDbClient))
            {
                checkDetails.Add("server.address", _options.UriEndpoint);
                documentDbClient = new DocumentClient(new Uri(_options.UriEndpoint), _options.PrimaryKey);

                if (!_connections.TryAdd(_options.UriEndpoint, documentDbClient))
                {
                    documentDbClient.Dispose();
                    documentDbClient = _connections[_options.UriEndpoint];
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.DatabaseName) && !string.IsNullOrWhiteSpace(_options.CollectionName))
            {
                await documentDbClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_options.DatabaseName, _options.CollectionName)).ConfigureAwait(false);
            }
            else
            {
                await documentDbClient.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }
}
