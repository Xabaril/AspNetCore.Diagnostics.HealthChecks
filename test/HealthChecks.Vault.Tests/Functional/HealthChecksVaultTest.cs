using System.Net;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace HealthChecks.Vault.Tests.Functional;

public class HealthChecksVaultTest : IClassFixture<Fixtures.VaultContainerFixture>
{
    private readonly Fixtures.VaultContainerFixture _vaultContainerFixture;

    public HealthChecksVaultTest(Fixtures.VaultContainerFixture vaultContainerFixture)
    {
        _vaultContainerFixture = vaultContainerFixture;
        Task.Run(() => _vaultContainerFixture.StartContainerAsync()).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task VaultHealthCheck_Should_Available()
    {
        var vaultAddress = _vaultContainerFixture.VaultAddress;
        var rootToken = _vaultContainerFixture.RootToken;

        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(rootToken);
        var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
        IVaultClient vaultClient = new VaultClient(vaultClientSettings);

        var webHostBuilder = new WebHostBuilder()
       .ConfigureServices(services =>
       {
           services.AddHealthChecks()
            .AddVaultHealthCheck(vaultAddress, rootToken!, "vault");
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
}
