using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Consul
{
    public class ConsulHealthCheck : IHealthCheck
    {
        private readonly string _consulHost;
        private readonly int _consulPort;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ConsulHealthCheck(string consulHost, int consulPort)
        {
            _consulHost = consulHost;
            _consulPort = consulPort;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _httpClient.GetAsync($"http://{_consulHost}:{_consulPort}/v1/status/leader", cancellationToken);
                return result.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}