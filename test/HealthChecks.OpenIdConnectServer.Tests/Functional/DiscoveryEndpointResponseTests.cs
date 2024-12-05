namespace HealthChecks.OpenIdConnectServer.Tests.Functional;

public class discovery_endpoint_response_should
{
    [Fact]
    public void be_invalid_when_issuer_is_missing()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = string.Empty,
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'issuer' must be set!");
    }

    [Fact]
    public void be_invalid_when_authorization_endpoint_is_missing()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = string.Empty,
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'authorization_endpoint' must be set!");
    }

    [Fact]
    public void be_invalid_when_jwks_uri_is_missing()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = string.Empty,
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'jwks_uri' must be set!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("id_token", "id_token token")]
    [InlineData("code", "id_token token")]
    public void be_invalid_when_required_response_types_supported_are_missing(params string[] responseTypesSupported)
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = RandomString,
            ResponseTypesSupported = responseTypesSupported,
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'response_types_supported' must contain the following values: code,id_token!");
    }

    [Fact]
    public void be_invalid_when_combined_response_types_supported_are_missing()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = RandomString,
            ResponseTypesSupported = ["id_token", "code"],
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'response_types_supported' must be one of the following values: token id_token,id_token token!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("some-value")]
    public void be_invalid_when_required_subject_types_supported_are_missing(string subjectTypesSupported)
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = RandomString,
            ResponseTypesSupported = REQUIRED_RESPONSE_TYPES,
            SubjectTypesSupported = [subjectTypesSupported],
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'subject_types_supported' must be one of the following values: pairwise,public!");
    }

    [Fact]
    public void be_invalid_when_required_id_token_signing_alg_values_supported_is_missing()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = RandomString,
            ResponseTypesSupported = REQUIRED_RESPONSE_TYPES,
            SubjectTypesSupported = OidcConstants.REQUIRED_SUBJECT_TYPES,
            SigningAlgorithmsSupported = [string.Empty],
        };

        Action validate = () => response.ValidateResponse();

        validate
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Invalid discovery response - 'id_token_signing_alg_values_supported' must contain the following values: RS256!");
    }

    [Fact]
    public void be_valid_when_all_required_values_are_provided()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = RandomString,
            ResponseTypesSupported = REQUIRED_RESPONSE_TYPES,
            SubjectTypesSupported = OidcConstants.REQUIRED_SUBJECT_TYPES,
            SigningAlgorithmsSupported = OidcConstants.REQUIRED_ALGORITHMS,
        };

        Action validate = () => response.ValidateResponse();

        validate.ShouldNotThrow();
    }

    [Fact]
    public void be_valid_when_dynamic_openid_provider_check_is_disabled_and_required_response_types_supported_are_missing()
    {
        var response = new DiscoveryEndpointResponse
        {
            Issuer = RandomString,
            AuthorizationEndpoint = RandomString,
            JwksUri = RandomString,
            SubjectTypesSupported = OidcConstants.REQUIRED_SUBJECT_TYPES,
            SigningAlgorithmsSupported = OidcConstants.REQUIRED_ALGORITHMS,
        };

        Action validate = () => response.ValidateResponse(isDynamicOpenIdProvider: false);

        validate.ShouldNotThrow();
    }

    private static string RandomString => Guid.NewGuid().ToString();

    private static readonly string[] REQUIRED_RESPONSE_TYPES = OidcConstants.REQUIRED_RESPONSE_TYPES.Concat(["id_token token"]).ToArray();
}
