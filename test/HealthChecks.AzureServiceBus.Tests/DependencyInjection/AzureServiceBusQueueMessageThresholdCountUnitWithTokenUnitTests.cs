namespace HealthChecks.AzureServiceBus.Tests.DependencyInjection;

public class azure_service_bus_queue_message_threshold_registration_with_token_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThreshold("cnn", "queueName", new MockTokenCredentials());

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azurequeuethreshold");
        check.GetType().ShouldBe(typeof(AzureServiceBusQueueMessageCountThresholdHealthCheck));
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThreshold("cnn", "queueName", new MockTokenCredentials(),
            name: "azureservicebusqueuemessagethresholdcheck");

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azureservicebusqueuemessagethresholdcheck");
        check.GetType().ShouldBe(typeof(AzureServiceBusQueueMessageCountThresholdHealthCheck));
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThreshold(string.Empty, string.Empty, new MockTokenCredentials());

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        Assert.Throws<ArgumentException>(() => registration.Factory(serviceProvider));
    }
}
