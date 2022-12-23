using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IdSvr
{
    public class IdSvrHealthCheck : IHealthCheck
    {
        private const string IDSVR_DISCOVER_CONFIGURATION_SEGMENT = ".well-known/openid-configuration";

        private readonly Func<HttpClient> _httpClientFactory;

        public IdSvrHealthCheck(Func<HttpClient> httpClientFactory)
        {
            _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory();
                using var response = await httpClient.GetAsync(IDSVR_DISCOVER_CONFIGURATION_SEGMENT, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy()
                    : new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
