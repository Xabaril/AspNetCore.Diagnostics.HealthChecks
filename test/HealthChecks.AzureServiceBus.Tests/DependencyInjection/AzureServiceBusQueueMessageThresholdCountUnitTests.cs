namespace HealthChecks.AzureServiceBus.Tests.DependencyInjection;

public class azure_service_bus_queue_message_threshold_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThreshold("cnn", "queueName");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azurequeuethreshold");
        check.ShouldBeOfType<AzureServiceBusQueueMessageCountThresholdHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThreshold("cnn", "queueName",
            name: "azureservicebusqueuemessagethresholdcheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azureservicebusqueuemessagethresholdcheck");
        check.ShouldBeOfType<AzureServiceBusQueueMessageCountThresholdHealthCheck>();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(string.Empty, string.Empty);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var factory = () => registration.Factory(serviceProvider);

        factory.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("azureservicebusqueuemessagethresholdcheck")]
    public void add_health_check_with_namespace_when_properly_configured(string? name)
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThresholdWithNamespace("cnn", "queueName", name);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(name ?? "azurequeuethreshold");
        check.ShouldBeOfType<AzureServiceBusQueueMessageCountThresholdHealthCheck>();
    }
}
