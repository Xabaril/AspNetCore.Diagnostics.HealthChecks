namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_topic_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic("cnn", "topicName");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuretopic");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic("cnn", "topic", name: "azuretopiccheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuretopiccheck");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic(string.Empty, string.Empty);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        var exception = Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("options");
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        bool connectionStringFactoryCalled = false, topicNameFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic(_ =>
                {
                    connectionStringFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    topicNameFactoryCalled = true;
                    return "topicName";
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuretopic");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
        connectionStringFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("azuretopiccheck")]
    public void add_health_check_with_namespace_when_properly_configured(string? name)
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopicWithNamespace("cnn", "topicName", name);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(name ?? "azuretopic");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
    }
}
