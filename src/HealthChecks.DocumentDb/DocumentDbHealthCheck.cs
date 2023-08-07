using System.Collections.Concurrent;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DocumentDb;

public class DocumentDbHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, DocumentClient> _connections = new();
    private readonly DocumentDbOptions _options;

    public DocumentDbHealthCheck(DocumentDbOptions documentDbOptions)
    {
        Guard.ThrowIfNull(documentDbOptions.UriEndpoint);
        Guard.ThrowIfNull(documentDbOptions.PrimaryKey);

        _options = documentDbOptions;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connections.TryGetValue(_options.UriEndpoint, out var documentDbClient))
            {
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

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
