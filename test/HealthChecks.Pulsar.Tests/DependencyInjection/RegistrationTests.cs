using System.Net;
using DotPulsar;

namespace HealthChecks.Pulsar.Tests.DependencyInjection;

public class pulsar_registration_should
{
    [Fact]
    public async Task add_health_check_when_properly_configured()
    {
        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri("pulsar://localhost:1234"))
            .Build();

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddPulsar(_ => client);

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("pulsar");
        check.ShouldBeOfType<PulsarHealthCheck>();
    }
    [Fact]
    public async Task add_named_health_check_when_properly_configured()
    {
        await using var client = PulsarClient.Builder()
            .ServiceUrl(new Uri("pulsar://localhost:1234"))
            .Build();

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddPulsar(_ => client, name: "my-pulsar-group");

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-pulsar-group");
        check.ShouldBeOfType<PulsarHealthCheck>();
    }
}
