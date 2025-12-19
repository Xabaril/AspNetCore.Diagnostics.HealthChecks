using Azure.Core;
using HealthChecks.AzureSearch.DependencyInjection;

namespace HealthChecks.AzureSearch.Tests.DependencyInjection;

public class azuresearch_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_with_auth_key()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureSearch(_ => { _.Endpoint = "endpoint"; _.IndexName = "indexName"; _.AuthKey = "authKey"; });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresearch");
        check.ShouldBeOfType<AzureSearchHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_with_token_credential()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureSearch(_ =>
            {
                _.Endpoint = "endpoint";
                _.IndexName = "indexName";
                _.TokenCredential = new MockTokenCredentials();
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresearch");
        check.ShouldBeOfType<AzureSearchHealthCheck>();
    }

    [Fact]
    public void throw_exception_when_no_auth_key_and_no_token_credential_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureSearch(_ =>
            {
                _.Endpoint = "endpoint";
                _.IndexName = "indexName";
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registration = options.Value.Registrations.First();

        var ex = Should.Throw<ArgumentException>(() =>
        {
            _ = registration.Factory(serviceProvider);
        });

        ex.Message.ShouldBe("Either AuthKey or TokenCredential must be set");
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureSearch(_ => { _.Endpoint = "endpoint"; _.IndexName = "indexName"; _.AuthKey = "authKey"; }, name: "my-azuresearch-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-azuresearch-group");
        check.ShouldBeOfType<AzureSearchHealthCheck>();
    }

    public class MockTokenCredentials : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
