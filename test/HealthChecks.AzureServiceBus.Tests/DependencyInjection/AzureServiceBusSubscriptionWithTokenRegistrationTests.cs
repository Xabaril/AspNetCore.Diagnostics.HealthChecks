using Azure.Identity;
using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_subscription_registration_with_token_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription("cnn", "topicName", "subscriptionName", new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscription");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_options_when_properly_configured()
    {
        var services = new ServiceCollection();
        AzureServiceBusSubscriptionOptions? configurationOptions = null;
        bool configurationCalled = false;

        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(
                "endpoint://",
                "topicName",
                "subscriptionName",
                new AzureCliCredential(),
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscription");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        var services = new ServiceCollection();
        bool endpointFactoryCalled = false,
            topicNameFactoryCalled = false,
            subscriptionNameFactoryCalled = false,
            tokenCredentialsFactoryCalled = false;
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(_ =>
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
                    subscriptionNameFactoryCalled = true;
                    return "subscriptionName";
                },
                _ =>
                {
                    tokenCredentialsFactoryCalled = true;
                    return new AzureCliCredential();
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscription");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        endpointFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
        subscriptionNameFactoryCalled.ShouldBeTrue();
        tokenCredentialsFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_using_factories_with_options_when_properly_configured()
    {
        var services = new ServiceCollection();
        AzureServiceBusSubscriptionOptions? configurationOptions = null;
        bool configurationCalled = false;

        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(
                _ => "endpoint://",
                _ => "topicName",
                _ => "subscriptionName",
                _ => new AzureCliCredential(),
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscription");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription("cnn", "topic", "subscriptionName", new AzureCliCredential(),
                name: "azuresubscriptioncheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscriptioncheck");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_using_factories_when_properly_configured()
    {
        var services = new ServiceCollection();
        bool endpointFactoryCalled = false,
            topicNameFactoryCalled = false,
            subscriptionNameFactoryCalled = false,
            tokenCredentialsFactoryCalled = false;
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(_ =>
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
                    subscriptionNameFactoryCalled = true;
                    return "subscriptionName";
                },
                _ =>
                {
                    tokenCredentialsFactoryCalled = true;
                    return new AzureCliCredential();
                },
                name: "azuresubscriptioncheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscriptioncheck");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        endpointFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
        subscriptionNameFactoryCalled.ShouldBeTrue();
        tokenCredentialsFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(string.Empty, string.Empty, string.Empty, new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        var exception = Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("options");
    }
}
