using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.DocumentDb
{
    public class DocumentDbHealthCheck
        : IHealthCheck
    {
        private static readonly ConcurrentDictionary<string, DocumentClient> _connections = new ConcurrentDictionary<string, DocumentClient>();

        private readonly DocumentDbOptions _documentDbOptions = new DocumentDbOptions();
        public DocumentDbHealthCheck(DocumentDbOptions documentDbOptions)
        {
            _documentDbOptions.UriEndpoint = documentDbOptions.UriEndpoint ?? throw new ArgumentNullException(nameof(documentDbOptions.UriEndpoint));
            _documentDbOptions.PrimaryKey = documentDbOptions.PrimaryKey ?? throw new ArgumentNullException(nameof(documentDbOptions.PrimaryKey));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                DocumentClient documentDbClient;

                if (!_connections.TryGetValue(_documentDbOptions.UriEndpoint, out documentDbClient))
                {
                    documentDbClient = new DocumentClient(new Uri(_documentDbOptions.UriEndpoint), _documentDbOptions.PrimaryKey);

                    if (!_connections.TryAdd(_documentDbOptions.UriEndpoint, documentDbClient))
                    {
                        documentDbClient.Dispose();
                        documentDbClient = _connections[_documentDbOptions.UriEndpoint];
                    }
                }
                await documentDbClient.OpenAsync(cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
