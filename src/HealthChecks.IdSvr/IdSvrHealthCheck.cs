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
        const string IDSVR_DISCOVER_CONFIGURATION_SEGMENT = ".well-known/openid-configuration";

        private readonly Func<HttpClient> _httpClientFactory;
        private readonly TimeSpan _timeout;

        public IdSvrHealthCheck(Func<HttpClient> httpClientFactory, TimeSpan timeout)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    var httpClient = _httpClientFactory();
                    var response = await httpClient.GetAsync(IDSVR_DISCOVER_CONFIGURATION_SEGMENT, timeoutCancellationTokenSource.Token);

                    if (!response.IsSuccessStatusCode)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content { (await response.Content.ReadAsStringAsync())}");
                    }

                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}
