using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.DocumentDb
{
    public class DocumentDbHealthCheck 
        : IHealthCheck
    {
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
                using (var documentDbClient = new DocumentClient(
                    new Uri(_documentDbOptions.UriEndpoint),
                    _documentDbOptions.PrimaryKey))
                {
                    await documentDbClient.OpenAsync();
                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
