namespace HealthChecks.IdSvr;

internal class OidcConstants
{
    internal const string ISSUER = "issuer";

    internal const string AUTHORIZATION_ENDPOINT = "authorization_endpoint";

    internal const string JWKS_URI = "jwks_uri";

    internal const string RESPONSE_TYPES_SUPPORTED = "response_types_supported";

    internal const string SUBJECT_TYPES_SUPPORTED = "subject_types_supported";

    internal const string ALGORITHMS_SUPPORTED = "id_token_signing_alg_values_supported";

    internal static string[] REQUIRED_RESPONSE_TYPES => new[] { "code", "id_token" };

    internal static string[] REQUIRED_COMBINED_RESPONSE_TYPES => new[] { "token id_token", "id_token token" };

    internal static string[] REQUIRED_SUBJECT_TYPES => new[] { "pairwise", "public" };

    internal static string[] REQUIRED_ALGORITHMS => new[] { "RS256" };
}
