using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthChecks.ClickHouse.Tests.DependencyInjection;

public class clickHouse_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddClickHouse("connectionstring");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("clickHouse");
        check.GetType().Should().Be(typeof(ClickHouseHealthCheck));
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddClickHouse("connectionstring", name: "my-click-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("my-click-1");
        check.GetType().Should().Be(typeof(ClickHouseHealthCheck));
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddClickHouse(_ =>
            {
                factoryCalled = true;
                return "connectionstring";
            }, name: "my-click-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.Should().Be("my-click-1");
        check.GetType().Should().Be(typeof(ClickHouseHealthCheck));
        factoryCalled.Should().BeTrue();
    }
}
