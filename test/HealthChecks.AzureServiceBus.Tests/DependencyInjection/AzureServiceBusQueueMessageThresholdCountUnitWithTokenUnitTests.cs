using Azure.Core;

namespace HealthChecks.AzureServiceBus.Tests.DependencyInjection;

public class azure_service_bus_queue_message_threshold_registration_with_token_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueMessageCountThreshold("cnn", "queueName", new MockTokenCredentials());

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
            .AddAzureServiceBusQueueMessageCountThreshold("cnn", "queueName", new MockTokenCredentials(),
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
            .AddAzureServiceBusQueueMessageCountThreshold(string.Empty, string.Empty, new MockTokenCredentials());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var factory = () => registration.Factory(serviceProvider);

        factory.ShouldThrow<ArgumentException>();
    }
}

public class MockTokenCredentials : TokenCredential
{
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) => throw new NotImplementedException();
    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) => throw new NotImplementedException();
}
