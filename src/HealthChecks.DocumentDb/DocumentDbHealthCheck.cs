using System.Collections.Concurrent;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DocumentDb;

public class DocumentDbHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, DocumentClient> _connections = new();
    private readonly string _uriEndpoint;
    private readonly string _primaryKey;
    private readonly string? _databaseName;
    private readonly string? _collectionName;

    public DocumentDbHealthCheck(DocumentDbOptions documentDbOptions)
    {
        _uriEndpoint = Guard.ThrowIfNull(documentDbOptions.UriEndpoint);
        _primaryKey = Guard.ThrowIfNull(documentDbOptions.PrimaryKey);
        _databaseName = documentDbOptions.DatabaseName;
        _collectionName = documentDbOptions.CollectionName;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connections.TryGetValue(_uriEndpoint, out var documentDbClient))
            {
                documentDbClient = new DocumentClient(new Uri(_uriEndpoint), _primaryKey);

                if (!_connections.TryAdd(_uriEndpoint, documentDbClient))
                {
                    documentDbClient.Dispose();
                    documentDbClient = _connections[_uriEndpoint];
                }
            }

            if (!string.IsNullOrWhiteSpace(_databaseName) && !string.IsNullOrWhiteSpace(_collectionName))
            {
                await documentDbClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName)).ConfigureAwait(false);
            }
            else
            {
                await documentDbClient.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
