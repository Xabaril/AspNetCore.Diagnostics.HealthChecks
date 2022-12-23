using Azure.Identity;

namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_queue_registration_with_token_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("cnn", "queueName", new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azurequeue");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        var services = new ServiceCollection();
        bool endpointFactoryCalled = false, queueNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ =>
                {
                    endpointFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    queueNameFactoryCalled = true;
                    return "queueName";
                },
                _ =>
                {
                    tokenCredentialFactoryCalled = true;
                    return new AzureCliCredential();
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azurequeue");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        endpointFactoryCalled.ShouldBeTrue();
        queueNameFactoryCalled.ShouldBeTrue();
        tokenCredentialFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("cnn", "queueName", new AzureCliCredential(),
                name: "azureservicebusqueuecheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azureservicebusqueuecheck");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_using_factories_when_properly_configured()
    {
        var services = new ServiceCollection();
        bool endpointFactoryCalled = false, queueNameFactoryCalled = false, tokenCredentialFactoryCalled = false;
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ =>
                {
                    endpointFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    queueNameFactoryCalled = true;
                    return "queueName";
                },
                _ =>
                {
                    tokenCredentialFactoryCalled = true;
                    return new AzureCliCredential();
                },
                "azureservicebusqueuecheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azureservicebusqueuecheck");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        endpointFactoryCalled.ShouldBeTrue();
        queueNameFactoryCalled.ShouldBeTrue();
        tokenCredentialFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(string.Empty, string.Empty, new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        var exception = Should.Throw<ArgumentNullException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("endpoint");
    }
}
