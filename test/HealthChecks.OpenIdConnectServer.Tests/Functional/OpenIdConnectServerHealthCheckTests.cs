using System.Net;

namespace HealthChecks.OpenIdConnectServer.Tests.Functional;

public class oidc_server_healthcheck_should(KeycloakContainerFixture keycloakFixture) : IClassFixture<KeycloakContainerFixture>
{
    [Fact]
    public async Task be_unhealthy_if_oidc_server_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddOpenIdConnectServer(new Uri("http://localhost:7777"), tags: ["oidcserver"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oidcserver")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_oidc_server_is_available()
    {
        string baseAddress = keycloakFixture.GetBaseAddress();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddOpenIdConnectServer(new Uri(baseAddress), tags: ["oidcserver"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oidcserver")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
