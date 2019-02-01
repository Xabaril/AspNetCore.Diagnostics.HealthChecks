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
        public ConsulHealthCheck(ConsulOptions options, Func<HttpClient> httpClientFactory)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClientFactory()
                    .GetAsync($"{(_options.RequireHttps ? "https" : "http")}://{_options.HostName}:{_options.Port}/v1/status/leader", cancellationToken);

                return result.IsSuccessStatusCode ? HealthCheckResult.Healthy() : new HealthCheckResult(context.Registration.FailureStatus, description: "Consul is not responding successfull status code.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}