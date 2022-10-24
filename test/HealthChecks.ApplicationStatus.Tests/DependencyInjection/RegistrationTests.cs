using HealthChecks.ApplicationStatus.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.ApplicationStatus.Tests.DependencyInjection;

public class applicationstatus_registration_should
{
    [Fact]
    public void add_health_check_no_direct_service_argument_when_properly_configured()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<IHostApplicationLifetime, TestHostApplicationLifeTime>()
            .AddHealthChecks()
            .AddApplicationStatus();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("applicationstatus");
        check.GetType().Should().Be(typeof(ApplicationStatusHealthCheck));
    }

    [Fact]
    public void add_named_health_check_no_direct_service_argument_when_properly_configured()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton<IHostApplicationLifetime, TestHostApplicationLifeTime>()
            .AddHealthChecks()
            .AddApplicationStatus(name: "custom-status");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("custom-status");
        check.GetType().Should().Be(typeof(ApplicationStatusHealthCheck));
    }

    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddApplicationStatus(new TestHostApplicationLifeTime());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("applicationstatus");
        check.GetType().Should().Be(typeof(ApplicationStatusHealthCheck));
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddApplicationStatus(new TestHostApplicationLifeTime(), name: "custom-status");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("custom-status");
        check.GetType().Should().Be(typeof(ApplicationStatusHealthCheck));
    }
}
