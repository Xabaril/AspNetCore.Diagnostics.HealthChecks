using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
                var client = _httpClientFactory();

                if (_options.Username != String.Empty && _options.Password != String.Empty) {
                    var authHeaderValue = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", _options.Username, _options.Password)));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
                }

                var result = await client
                    .GetAsync($"{(_options.RequireHttps ? "https" : "http")}://{_options.HostName}:{_options.Port}/v1/status/leader", cancellationToken);

                return result.IsSuccessStatusCode ? HealthCheckResult.Healthy() : new HealthCheckResult(context.Registration.FailureStatus, description: "Consul response was not a successful HTTP status code");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}