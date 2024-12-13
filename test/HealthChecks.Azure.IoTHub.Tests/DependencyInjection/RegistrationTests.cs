using Azure.Identity;
using Microsoft.Azure.Devices;

namespace HealthChecks.Azure.IoTHub.Tests.DependencyInjection;

public class azure_iothub_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(sp => ServiceClient.Create("iot-hub-hostname", new DefaultAzureCredential()))
            .AddHealthChecks()
            .AddAzureIoTHubServiceClient(name: "iothub");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("iothub");
        check.ShouldBeOfType<IoTHubServiceClientHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(sp => RegistryManager.Create("iot-hub-hostname", new DefaultAzureCredential()))
            .AddHealthChecks()
            .AddAzureIoTHubRegistryReadCheck(name: "iothubcheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("iothubcheck");
        check.ShouldBeOfType<IoTHubRegistryManagerHealthCheck>();
    }
}
