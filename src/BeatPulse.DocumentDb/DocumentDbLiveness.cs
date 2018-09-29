using BeatPulse.Core;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatPulse.DocumentDb
{
    public class DocumentDbLiveness : IBeatPulseLiveness
    {
        private readonly DocumentDbOptions _documentDbOptions = new DocumentDbOptions();
        private readonly ILogger<DocumentDbLiveness> _logger;

        public DocumentDbLiveness(DocumentDbOptions documentDbOptions, ILogger<DocumentDbLiveness> logger = null)
        {
            _documentDbOptions.UriEndpoint = documentDbOptions.UriEndpoint ?? throw new ArgumentNullException(nameof(documentDbOptions.UriEndpoint));
            _documentDbOptions.PrimaryKey = documentDbOptions.PrimaryKey ?? throw new ArgumentNullException(nameof(documentDbOptions.PrimaryKey));
            _logger = logger;
        }

        public async Task<LivenessResult> IsHealthy(LivenessExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogDebug($"{nameof(DocumentDbLiveness)} is checking DocumentDb database availability.");

                using (var documentDbClient = new DocumentClient(
                    new Uri(_documentDbOptions.UriEndpoint),
                    _documentDbOptions.PrimaryKey))
                {
                    await documentDbClient.OpenAsync();

                    _logger?.LogDebug($"The {nameof(DocumentDbLiveness)} check success.");

                    return LivenessResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug($"The {nameof(DocumentDbLiveness)} check fail with the exception {ex.ToString()}.");

                return LivenessResult.UnHealthy(ex);
            }
        }
    }
}
