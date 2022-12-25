using System.Collections.Concurrent;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.DocumentDb
{
    public class DocumentDbHealthCheck : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, DocumentClient> _connections = new();
        private readonly DocumentDbOptions _documentDbOptions = new();

        public DocumentDbHealthCheck(DocumentDbOptions documentDbOptions)
        {
            _documentDbOptions.UriEndpoint = Guard.ThrowIfNull(documentDbOptions.UriEndpoint);
            _documentDbOptions.PrimaryKey = Guard.ThrowIfNull(documentDbOptions.PrimaryKey);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_connections.TryGetValue(_documentDbOptions.UriEndpoint, out var documentDbClient))
                {
                    documentDbClient = new DocumentClient(new Uri(_documentDbOptions.UriEndpoint), _documentDbOptions.PrimaryKey);

                    if (!_connections.TryAdd(_documentDbOptions.UriEndpoint, documentDbClient))
                    {
                        documentDbClient.Dispose();
                        documentDbClient = _connections[_documentDbOptions.UriEndpoint];
                    }
                }
                await documentDbClient.OpenAsync(cancellationToken).ConfigureAwait(false);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
