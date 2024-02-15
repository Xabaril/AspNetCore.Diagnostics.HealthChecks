using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IdSvr;

public class IdSvrHealthCheck : IHealthCheck
{
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly string _discoverConfigurationSegment;
    private readonly bool _isDynamicOpenIdProvider;

    public IdSvrHealthCheck(Func<HttpClient> httpClientFactory)
        : this(httpClientFactory, IdSvrHealthCheckBuilderExtensions.IDSVR_DISCOVER_CONFIGURATION_SEGMENT, false)
    {
    }

    public IdSvrHealthCheck(Func<HttpClient> httpClientFactory, string discoverConfigurationSegment, bool isDynamicOpenIdProvider)
    {
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
        _discoverConfigurationSegment = discoverConfigurationSegment;
        _isDynamicOpenIdProvider = isDynamicOpenIdProvider;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory();
            using var response = await httpClient.GetAsync(_discoverConfigurationSegment, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
            }

            var discoveryResponse = await response
                   .Content
                   .ReadFromJsonAsync<DiscoveryEndpointResponse>()
                   .ConfigureAwait(false)
               ?? throw new ArgumentException("Could not deserialize to discovery endpoint response!");

            discoveryResponse.ValidateResponse(_isDynamicOpenIdProvider);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
