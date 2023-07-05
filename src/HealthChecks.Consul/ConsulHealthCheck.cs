using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Consul;

public class ConsulHealthCheck : IHealthCheck
{
    private readonly ConsulOptions _options;
    private readonly Func<HttpClient> _httpClientFactory;

    public ConsulHealthCheck(ConsulOptions options, Func<HttpClient> httpClientFactory)
    {
        _options = Guard.ThrowIfNull(options);
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory();
            if (_options.RequireBasicAuthentication)
            {
                byte[] credentials = Encoding.ASCII.GetBytes($"{_options.Username}:{_options.Password}");
                string authHeaderValue = Convert.ToBase64String(credentials);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            }
            using var result = await client.GetAsync($"{(_options.RequireHttps ? "https" : "http")}://{_options.HostName}:{_options.Port}/v1/status/leader", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            return result.IsSuccessStatusCode ? HealthCheckResult.Healthy() : new HealthCheckResult(context.Registration.FailureStatus, description: "Consul response was not a successful HTTP status code");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
