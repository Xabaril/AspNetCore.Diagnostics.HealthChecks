using HealthChecks.Hangfire;

namespace HealthChecks.Gremlin.Tests.DependencyInjection;

public class hangfire_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddHangfire(setup => setup.MaximumJobsFailed = 3);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("hangfire");
        check.ShouldBeOfType<HangfireHealthCheck>();
    }
    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddHangfire(setup => setup.MaximumJobsFailed = 3, name: "my-hangfire-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-hangfire-group");
        check.ShouldBeOfType<HangfireHealthCheck>();
    }
}
