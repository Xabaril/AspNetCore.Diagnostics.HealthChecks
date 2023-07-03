namespace HealthChecks.Consul.Tests.DependencyInjection;

public class consul_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddConsul(setup =>
            {
                setup.HostName = "hostname";
                setup.Port = 8500;
                setup.RequireHttps = false;
            });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("consul");
        check.ShouldBeOfType<ConsulHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddConsul(setup =>
            {
                setup.HostName = "hostname";
                setup.Port = 8500;
                setup.RequireHttps = false;
            }, name: "my-consul-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-consul-group");
        check.ShouldBeOfType<ConsulHealthCheck>();
    }
}
