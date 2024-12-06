using System.Net;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace HealthChecks.Vault.Tests.Functional;

public class vault_healthcheck_should : IClassFixture<Fixtures.VaultContainerFixture>
{
    private readonly Fixtures.VaultContainerFixture _vaultContainerFixture;

    public vault_healthcheck_should(Fixtures.VaultContainerFixture vaultContainerFixture)
    {
        _vaultContainerFixture = vaultContainerFixture;
    }

    [Fact]
    public async Task be_healthy_when_vault_is_available_using_client_factory()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddVault(
                        clientFactory: sp =>
                        {
                            var vaultAddress = _vaultContainerFixture.GetConnectionString();
                            var rootToken = _vaultContainerFixture.RootToken;
                            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(rootToken);
                            var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
                            IVaultClient vaultClient = new VaultClient(vaultClientSettings);
                            return vaultClient;
                        }, tags: new string[] { "vault" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("vault")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_vault_is_available_using_singleton()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var vaultAddress = _vaultContainerFixture.GetConnectionString();
                var rootToken = _vaultContainerFixture.RootToken;
                IAuthMethodInfo authMethod = new TokenAuthMethodInfo(rootToken);
                var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
                IVaultClient vaultClient = new VaultClient(vaultClientSettings);

                services
                    .AddSingleton(vaultClient)
                    .AddHealthChecks()
                    .AddVault(tags: new string[] { "vault" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("vault")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_vault_is_unavailable()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddVault(
                        clientFactory: sp => new VaultClient(new VaultClientSettings("http://localhost:8200", new TokenAuthMethodInfo("root"))), tags: new string[] { "vault" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("vault")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}

