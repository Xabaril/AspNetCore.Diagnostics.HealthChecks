namespace HealthChecks.Azure.IoTHub.Tests.DependencyInjection;

public class azure_iothub_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureIoTHub(options => options.AddConnectionString("the-iot-connection-string"));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("iothub");
        check.ShouldBeOfType<IoTHubHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
             .AddAzureIoTHub(options => options.AddConnectionString("the-iot-connection-string"), name: "iothubcheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("iothubcheck");
        check.ShouldBeOfType<IoTHubHealthCheck>();
    }
}
