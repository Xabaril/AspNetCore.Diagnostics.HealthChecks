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
    public async Task CheckVaultHealthAsync()
    {
        var vaultAddress = _vaultContainerFixture.VaultAddress;
        var rootToken = _vaultContainerFixture.RootToken;

        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(rootToken);
        var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
        IVaultClient vaultClient = new VaultClient(vaultClientSettings);

        try
        {
            // Check the health of the Vault server
            var healthStatus = await vaultClient.V1.System.GetHealthStatusAsync();

            // Assert the health status
            Assert.True(healthStatus.Initialized);
            Assert.False(healthStatus.Sealed);
            Console.WriteLine($"Vault is healthy: Initialized = {healthStatus.Initialized}, Sealed = {healthStatus.Sealed}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Health check failed: {ex.Message}");
        }
    }
}
