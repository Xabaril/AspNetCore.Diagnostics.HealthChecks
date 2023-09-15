namespace HealthChecks.Hazelcast.Tests.DependencyInjection;

public class hazelcast_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddHazelcast(new HazelcastHealthCheckOptions()
            {
                ConnectionHost = "127.0.0.1:5701"
            })
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("hazelcast");
        check.ShouldBeOfType<HazelcastHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddHazelcast(new HazelcastHealthCheckOptions()
            {
                ConnectionHost = "127.0.0.1:5701"
            }, name: "my-hazelcast-1")
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-hazelcast-1");
        check.ShouldBeOfType<HazelcastHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_action_options_when_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddHazelcast(options =>
            {
                options.ConnectionHost = "127.0.0.1:5701";
                options.ClusterName = "dev";
                options.ClientName = "my-client";
                options.ConnectionTimeout = TimeSpan.FromMilliseconds(500);
            })
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("hazelcast");
        check.ShouldBeOfType<HazelcastHealthCheck>();
    }

    [Fact]
    public void throw_exception_when_not_properly_configured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddHazelcast(new HazelcastHealthCheckOptions())
            .Services;

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        
        var exception = Record.Exception(() => registration.Factory(serviceProvider));
        var anException = exception.ShouldBeOfType<ArgumentNullException>();
        anException.ParamName.ShouldBeEquivalentTo("options.ConnectionHost");
    }
}
