using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.IdSvr
{
    public class IdSvrHealthCheck
        : IHealthCheck
    {
        const string IDSVR_DISCOVER_CONFIGURATION_SEGMENT = ".well-known/openid-configuration";

        private readonly Uri _idSvrUri;
        private readonly ILogger<IdSvrHealthCheck> _logger;

        public IdSvrHealthCheck(Uri idSvrUri, ILogger<IdSvrHealthCheck> logger = null)
        {
            _idSvrUri = idSvrUri ?? throw new ArgumentNullException(nameof(idSvrUri));
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation($"{nameof(IdSvrHealthCheck)} is checking the IdSvr on {_idSvrUri}.");

                using (var httpClient = new HttpClient() { BaseAddress = _idSvrUri })
                {
                    var response = await httpClient.GetAsync(IDSVR_DISCOVER_CONFIGURATION_SEGMENT);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger?.LogWarning($"The {nameof(IdSvrHealthCheck)} check failed for server {_idSvrUri}.");

                        return HealthCheckResult.Failed("Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content { (await response.Content.ReadAsStringAsync())}");
                    }

                    _logger?.LogInformation($"The {nameof(IdSvrHealthCheck)} check success.");

                    return HealthCheckResult.Passed();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"The {nameof(IdSvrHealthCheck)} check fail for IdSvr with the exception {ex.ToString()}.");

                return HealthCheckResult.Failed(exception:ex);
            }
        }
    }
}
