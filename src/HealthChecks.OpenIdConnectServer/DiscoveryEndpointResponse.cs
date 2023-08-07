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
    public string[] ResponseTypesSupported { get; set; } = null!;

    [JsonPropertyName(OidcConstants.SUBJECT_TYPES_SUPPORTED)]
    public string[] SubjectTypesSupported { get; set; } = null!;

    [JsonPropertyName(OidcConstants.ALGORITHMS_SUPPORTED)]
    public string[] IdTokenSigningAlgValuesSupported { get; set; } = null!;

    /// <summary>
    /// Validates Discovery response according to the <see href="https://openid.net/specs/openid-connect-discovery-1_0.html#ProviderMetadata">OpenID specification</see>
    /// </summary>
    public void ValidateResponse()
    {
        ValidateValue(Issuer, OidcConstants.ISSUER);
        ValidateValue(AuthorizationEndpoint, OidcConstants.AUTHORIZATION_ENDPOINT);
        ValidateValue(JwksUri, OidcConstants.JWKS_URI);

        ValidateRequiredValues(ResponseTypesSupported, OidcConstants.RESPONSE_TYPES_SUPPORTED, OidcConstants.REQUIRED_RESPONSE_TYPES);
        ValidateRequiredValues(SubjectTypesSupported, OidcConstants.SUBJECT_TYPES_SUPPORTED, OidcConstants.REQUIRED_SUBJECT_TYPES);
        ValidateRequiredValues(IdTokenSigningAlgValuesSupported, OidcConstants.ALGORITHMS_SUPPORTED, OidcConstants.REQUIRED_ALGORITHMS);
    }

    private static void ValidateValue(string value, string metadata)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(GetMissingValueExceptionMessage(metadata));
        }
    }

    private static void ValidateRequiredValues(IEnumerable<string> values, string metadata, string[] requiredValues)
    {
        if (values == null || !AnyValueContains(values, requiredValues))
        {
            throw new ArgumentException(GetMissingRequiredValuesExceptionMessage(metadata, requiredValues));
        }
    }

    private static bool AnyValueContains(IEnumerable<string> values, string[] requiredValues) =>
        values.Any(v => requiredValues.Contains(v));

    private static string GetMissingValueExceptionMessage(string value) =>
        $"Invalid discovery response - '{value}' must be set!";

    private static string GetMissingRequiredValuesExceptionMessage(string value, string[] requiredValues) =>
        $"Invalid discovery response - '{value}' must be one of the following values: {string.Join(",", requiredValues)}!";
}
