using Azure.Identity;

namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_topic_registration_with_token_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic("cnn", "topicName", new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuretopic");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        bool endpointFactoryCalled = false, topicNameFactoryCalled = false, tokenCredentialFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic(_ =>
                {
                    endpointFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    topicNameFactoryCalled = true;
                    return "topicName";
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

        registration.Name.ShouldBe("azuretopic");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
        endpointFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
        tokenCredentialFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic("cnn", "topic", new AzureCliCredential(), "azuretopiccheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuretopiccheck");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_using_factories_when_properly_configured()
    {
        bool endpointFactoryCalled = false, topicNameFactoryCalled = false, tokenCredentialFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic(_ =>
                {
                    endpointFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    topicNameFactoryCalled = true;
                    return "topicName";
                },
                _ =>
                {
                    tokenCredentialFactoryCalled = true;
                    return new AzureCliCredential();
                },
                name: "azuretopiccheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuretopiccheck");
        check.ShouldBeOfType<AzureServiceBusTopicHealthCheck>();
        endpointFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
        tokenCredentialFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusTopic(string.Empty, string.Empty, new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        var exception = Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("options");
    }
}
