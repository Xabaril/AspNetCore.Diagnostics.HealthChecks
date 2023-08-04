using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IdSvr;

public class IdSvrHealthCheck : IHealthCheck
{
    private readonly Func<HttpClient> _httpClientFactory;
    private readonly string _discoverConfigurationSegment;

    public IdSvrHealthCheck(Func<HttpClient> httpClientFactory)
        : this(httpClientFactory, IdSvrHealthCheckBuilderExtensions.IDSVR_DISCOVER_CONFIGURATION_SEGMENT)
    {
    }

    public IdSvrHealthCheck(Func<HttpClient> httpClientFactory, string discoverConfigurationSegment)
    {
        _httpClientFactory = Guard.ThrowIfNull(httpClientFactory);
        _discoverConfigurationSegment = discoverConfigurationSegment;
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
               ?? throw new ArgumentException("Could not deserialize to discover endpoint response!");

            ValidateResponse(discoveryResponse);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static void ValidateResponse(DiscoveryEndpointResponse response)
    {
        OidcValidationHelper.ValidateValue(response.Issuer, OidcConstants.ISSUER);
        OidcValidationHelper.ValidateValue(response.AuthorizationEndpoint, OidcConstants.AUTHORIZATION_ENDPOINT);
        OidcValidationHelper.ValidateValue(response.JwksUri, OidcConstants.JWKS_URI);

        OidcValidationHelper.ValidateRequiredValues(response.ResponseTypesSupported, OidcConstants.RESPONSE_TYPES_SUPPORTED, OidcConstants.REQUIRED_RESPONSE_TYPES);
        OidcValidationHelper.ValidateRequiredValues(response.SubjectTypesSupported, OidcConstants.SUBJECT_TYPES_SUPPORTED, OidcConstants.REQUIRED_SUBJECT_TYPES);
        OidcValidationHelper.ValidateRequiredValues(response.IdTokenSigningAlgValuesSupported, OidcConstants.ALGORITHMS_SUPPORTED, OidcConstants.REQUIRED_ALGORITHMS);
    }
}
