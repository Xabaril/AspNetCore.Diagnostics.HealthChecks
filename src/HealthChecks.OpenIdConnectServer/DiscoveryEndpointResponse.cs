using System.Text.Json.Serialization;

namespace HealthChecks.IdSvr;

internal class DiscoveryEndpointResponse
{
    [JsonPropertyName(OidcConstants.ISSUER)]
    public string Issuer { get; set; } = null!;

    [JsonPropertyName(OidcConstants.AUTHORIZATION_ENDPOINT)]
    public string AuthorizationEndpoint { get; set; } = null!;

    [JsonPropertyName(OidcConstants.JWKS_URI)]
    public string JwksUri { get; set; } = null!;

    [JsonPropertyName(OidcConstants.RESPONSE_TYPES_SUPPORTED)]
    public IEnumerable<string> ResponseTypesSupported { get; set; } = null!;

    [JsonPropertyName(OidcConstants.SUBJECT_TYPES_SUPPORTED)]
    public IEnumerable<string> SubjectTypesSupported { get; set; } = null!;

    [JsonPropertyName(OidcConstants.ALGORITHMS_SUPPORTED)]
    public IEnumerable<string> IdTokenSigningAlgValuesSupported { get; set; } = null!;
}
