using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.DocumentDb
{
    public class DocumentDbHealthCheck 
        : IHealthCheck
    {
        private readonly DocumentDbOptions _documentDbOptions = new DocumentDbOptions();
        private readonly ILogger<DocumentDbHealthCheck> _logger;

        public DocumentDbHealthCheck(DocumentDbOptions documentDbOptions, ILogger<DocumentDbHealthCheck> logger = null)
        {
            _documentDbOptions.UriEndpoint = documentDbOptions.UriEndpoint ?? throw new ArgumentNullException(nameof(documentDbOptions.UriEndpoint));
            _documentDbOptions.PrimaryKey = documentDbOptions.PrimaryKey ?? throw new ArgumentNullException(nameof(documentDbOptions.PrimaryKey));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug($"{nameof(DocumentDbHealthCheck)} is checking DocumentDb database availability.");

                using (var documentDbClient = new DocumentClient(
                    new Uri(_documentDbOptions.UriEndpoint),
                    _documentDbOptions.PrimaryKey))
                {
                    await documentDbClient.OpenAsync();

                    _logger?.LogDebug($"The {nameof(DocumentDbHealthCheck)} check success.");

                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"The {nameof(DocumentDbHealthCheck)} check fail with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
