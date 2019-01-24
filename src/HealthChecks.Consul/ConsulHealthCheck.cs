using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Consul
{
    public class ConsulHealthCheck : IHealthCheck
    {
        private readonly ConsulOptions _options;
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly TimeSpan _timeout;

        public ConsulHealthCheck(ConsulOptions options, Func<HttpClient> httpClientFactory, TimeSpan timeout)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    using (var result = await _httpClientFactory()
                        .GetAsync($"{(_options.RequireHttps ? "https" : "http")}://{_options.HostName}:{_options.Port}/v1/status/leader", timeoutCancellationTokenSource.Token))
                    {

                        return result.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                    }
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