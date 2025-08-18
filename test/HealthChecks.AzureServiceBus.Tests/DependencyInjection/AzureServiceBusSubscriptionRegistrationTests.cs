using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_subscription_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription("cnn", "topicName", "subscriptionName");

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
        AzureServiceBusSubscriptionHealthCheckHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription("cnn", "topicName", "subscriptionName",
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        bool connectionStringFactoryCalled = false,
            topicNameFactoryCalled = false,
            subscriptionNameFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(_ =>
                {
                    connectionStringFactoryCalled = true;
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
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscription");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        connectionStringFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
        subscriptionNameFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_using_factories_with_options_when_properly_configured()
    {
        AzureServiceBusSubscriptionHealthCheckHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(_ => "cnn", _ => "topicName", _ => "subscriptionName",
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

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
            .AddAzureServiceBusSubscription("cnn", "topic", "subscriptionName", name: "azuresubscriptioncheck");

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
        bool connectionStringFactoryCalled = false,
            topicNameFactoryCalled = false,
            subscriptionNameFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(_ =>
                {
                    connectionStringFactoryCalled = true;
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
                name: "azuresubscriptioncheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azuresubscriptioncheck");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
        connectionStringFactoryCalled.ShouldBeTrue();
        topicNameFactoryCalled.ShouldBeTrue();
        subscriptionNameFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void fail_when_no_health_check_configuration_provided()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscription(string.Empty, string.Empty, string.Empty);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();

        var exception = Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("options");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("azuresubscriptioncheck")]
    public void add_health_check_with_namespace_when_properly_configured(string? name)
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusSubscriptionWithNamespace("cnn", "topicName", "subscriptionName", name);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(name ?? "azuresubscription");
        check.ShouldBeOfType<AzureServiceBusSubscriptionHealthCheck>();
    }
}
