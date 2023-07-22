using Dapr.Client;

namespace HealthChecks.Dapr.Tests.DependencyInjection;

public class dapr_registration_should
{
    private const string _defaultCheckName = "dapr";

    [Fact]
    public void add_health_check_when_properly_configured_using_di()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DaprClientBuilder().Build());
        services.AddHealthChecks()
            .AddDapr();

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(_defaultCheckName);
        check.ShouldBeOfType<DaprHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_using_arguments()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddDapr(daprClient: new DaprClientBuilder().Build());

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(_defaultCheckName);
        check.ShouldBeOfType<DaprHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + _defaultCheckName;

        services.AddSingleton(new DaprClientBuilder().Build());
        services.AddHealthChecks()
            .AddDapr(name: customCheckName);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<DaprHealthCheck>();
    }
}
