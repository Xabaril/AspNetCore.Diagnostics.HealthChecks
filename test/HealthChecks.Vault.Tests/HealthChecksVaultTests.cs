namespace HealthChecks.Vault.Tests;

public class HealthChecksVaultTests
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

        var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        try
        {
            // Act
            var registration = healthCheckOptions.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            // Assert
            registration.Name.ShouldBe(_defaultCheckName);
            check.ShouldBeOfType<HealthChecksVault>();
        }
        finally
        {
            // Dispose the service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
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

        var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        try
        {
            // Act
            var registration = healthCheckOptions.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            // Assert
            registration.Name.ShouldBe(_defaultCheckName);
            check.ShouldBeOfType<HealthChecksVault>();
        }
        finally
        {
            // Dispose the service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
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

        var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        try
        {
            // Act
            var registration = healthCheckOptions.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            // Assert
            registration.Name.ShouldBe(_defaultCheckName);
            check.ShouldBeOfType<HealthChecksVault>();
        }
        finally
        {
            // Dispose the service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
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

        var serviceProvider = services.BuildServiceProvider();
        var healthCheckOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        try
        {
            // Act
            var registration = healthCheckOptions.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            // Assert
            registration.Name.ShouldBe(_defaultCheckName);
            check.ShouldBeOfType<HealthChecksVault>();
        }
        finally
        {
            // Dispose the service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    [Fact]
    public async Task CheckHealthAsync_VaultHealthy_ShouldReturnHealthyAsync()
    {
        var services = new ServiceCollection();

        var mockVaultClient = new Mock<IVaultClient>();
        services.AddSingleton<IVaultClient>(mockVaultClient.Object);
        var cancellationToken = CancellationToken.None;
        var mockSystemBackend = new Mock<ISystemBackend>();
        mockSystemBackend
            .Setup(m => m.GetHealthStatusAsync(false, 200, 429, 503, 501, null))
            .ReturnsAsync(new VaultSharp.V1.SystemBackend.HealthStatus { Initialized = true, Sealed = false });

        mockVaultClient.Setup(m => m.V1.System).Returns(mockSystemBackend.Object);

        var options = new VaultHealthCheckOptions()
            .UseBasicAuthentication("basic-token")
            .WithVaultAddress("http://127.0.0.1:8200");

        services.AddSingleton(options);
        services.AddSingleton<IHealthCheck, HealthChecksVault>();

        var serviceProvider = services.BuildServiceProvider();
        var healthChecksVault = serviceProvider.GetRequiredService<IHealthCheck>();

        try
        {
            // Act
            var result = await healthChecksVault.CheckHealthAsync(new HealthCheckContext()).ConfigureAwait(true);

            // Assert
            result.Status.ShouldBe(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy);
        }
        finally
        {
            // Dispose the service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
