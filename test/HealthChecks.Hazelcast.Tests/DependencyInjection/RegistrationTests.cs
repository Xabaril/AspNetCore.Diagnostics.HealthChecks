namespace HealthChecks.Hazelcast.Tests.DependencyInjection;

public class RegistrationTests
{

    [Fact]
    public void AddHazelcast_ShouldRegisterHealthCheck_WithConfiguredOptions()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddHazelcast(op =>
            {
                op.Port = 5721;
                op.Server = "localhost";
                op.ClusterNames = new() { "dev" };
            }, "hazelcast");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("hazelcast");
        check.ShouldBeOfType<HealthCheckServiceOptions>();
    }
}
