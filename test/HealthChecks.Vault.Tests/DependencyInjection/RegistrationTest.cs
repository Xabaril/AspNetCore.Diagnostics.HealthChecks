using HealthCheks.Vault;

namespace HealthChecks.Vault.Tests.DependencyInjection;

public class RegistrationTest
{
    protected readonly string _defaultCheckName = "vault";

    [Fact]
    public void AddHealthCheck_WithBasicAuthentication_ShouldBeProperlyConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new VaultHealthCheckOptions()
            .UseBasicAuthentication("basic-token")
            .WithVaultAddress("http://127.0.0.1:8200");
        services.AddSingleton(options);
        services.AddSingleton<IHealthCheck, HealthChecksVault>();

        services.AddHealthChecks()
            .AddCheck<HealthChecksVault>(_defaultCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        // Act
        var registration = healthCheckOptions.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        // Assert
        registration.Name.ShouldBe(_defaultCheckName);
        check.ShouldBeOfType<HealthChecksVault>();
    }

    [Fact]
    public void AddHealthCheck_WithRadiusAuthentication_ShouldBeProperlyConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new VaultHealthCheckOptions()
            .UseRadiusAuthentication("radius-password", "radius-username")
            .WithVaultAddress("http://127.0.0.1:8200");
        services.AddSingleton(options);
        services.AddSingleton<IHealthCheck, HealthChecksVault>();

        services.AddHealthChecks()
            .AddCheck<HealthChecksVault>(_defaultCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        // Act
        var registration = healthCheckOptions.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        // Assert
        registration.Name.ShouldBe(_defaultCheckName);
        check.ShouldBeOfType<HealthChecksVault>();

    }

    [Fact]
    public void AddHealthCheck_WithLdapAuthentication_ShouldBeProperlyConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new VaultHealthCheckOptions()
            .UseLdapAuthentication("ldap-password", "ldap-username")
            .WithVaultAddress("http://127.0.0.1:8200");
        services.AddSingleton(options);
        services.AddSingleton<IHealthCheck, HealthChecksVault>();

        services.AddHealthChecks()
            .AddCheck<HealthChecksVault>(_defaultCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();


        // Act
        var registration = healthCheckOptions.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        // Assert
        registration.Name.ShouldBe(_defaultCheckName);
        check.ShouldBeOfType<HealthChecksVault>();

    }

    [Fact]
    public void AddHealthCheck_WithOktaAuthentication_ShouldBeProperlyConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var options = new VaultHealthCheckOptions()
            .UseOktaAuthentication("okta-password", "okta-username")
            .WithVaultAddress("http://127.0.0.1:8200");
        services.AddSingleton(options);
        services.AddSingleton<IHealthCheck, HealthChecksVault>();

        services.AddHealthChecks()
            .AddCheck<HealthChecksVault>(_defaultCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();


        // Act
        var registration = healthCheckOptions.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        // Assert
        registration.Name.ShouldBe(_defaultCheckName);
        check.ShouldBeOfType<HealthChecksVault>();

    }
}
