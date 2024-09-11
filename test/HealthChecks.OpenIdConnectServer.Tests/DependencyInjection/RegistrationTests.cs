namespace HealthChecks.OpenIdConnectServer.Tests.DependencyInjection;

public class oidc_server_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddOpenIdConnectServer(new Uri("http://myoidcserver"));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("oidcserver");
        check.ShouldBeOfType<OpenIdConnectServerHealthCheck>();
    }
    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddOpenIdConnectServer(new Uri("http://myoidcserver"), name: "my-oidc-server-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-oidc-server-group");
        check.ShouldBeOfType<OpenIdConnectServerHealthCheck>();
    }
    [Fact]
    public void add_health_check_when_properly_configured_with_uri_provider()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddOpenIdConnectServer(sp => new Uri("http://myoidcserver"));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("oidcserver");
        check.ShouldBeOfType<OpenIdConnectServerHealthCheck>();
    }
    [Fact]
    public void add_named_health_check_when_properly_configured_with_uri_provider()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddOpenIdConnectServer(sp => new Uri("http://myoidcserver"), name: "my-oidc-server-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-oidc-server-group");
        check.ShouldBeOfType<OpenIdConnectServerHealthCheck>();
    }
}
