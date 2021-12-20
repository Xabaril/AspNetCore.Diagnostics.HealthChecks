using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.IdSvr
{
    public class IdSvrHealthCheck
        : IHealthCheck
    {
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly string _requestUri;
        public IdSvrHealthCheck(Func<HttpClient> httpClientFactory, string requestUri = ".well-known/openid-configuration")
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _requestUri = requestUri;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory();

                var response = await httpClient.GetAsync(_requestUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content { await response.Content.ReadAsStringAsync() }");
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}