using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace HealthChecks.Vault.Tests.DependencyInjection;

public class vault_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddVault(Factory);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("vault");
        check.ShouldBeOfType<VaultHealthChecks>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddVault(clientFactory: Factory, name: "vault");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("vault");
        check.ShouldBeOfType<VaultHealthChecks>();
    }

    private IVaultClient Factory(IServiceProvider _)
    {
        IAuthMethodInfo authMethod = new TokenAuthMethodInfo("token");
        return new VaultClient(new VaultClientSettings("http://localhost:8200", authMethod));
    }
}
