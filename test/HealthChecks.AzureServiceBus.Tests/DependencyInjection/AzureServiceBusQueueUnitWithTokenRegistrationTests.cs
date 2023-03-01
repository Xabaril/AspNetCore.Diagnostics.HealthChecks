using Azure.Identity;
using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_queue_registration_with_token_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("fullyQualifiedNamespace", "queueName", new AzureCliCredential());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azurequeue");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_options_when_properly_configured()
    {
        AzureServiceBusQueueHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("fullyQualifiedNamespace", "queueName", new AzureCliCredential(),
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
        configurationOptions.IsExceptionDetailsRequired.ShouldBeTrue();
    }
    [Fact]
    public void add_health_check_without_giving_exception_in_result_for_unhealthy_queues()
    {
        AzureServiceBusQueueHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("fullyQualifiedNamespace", "queueName", new AzureCliCredential(),
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                }, isExceptionDetailsRequired: false);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
        configurationOptions.IsExceptionDetailsRequired.ShouldBeFalse();
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        bool endpointFactoryCalled = false, queueNameFactoryCalled = false, tokenCredentialFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ =>
                {
                    endpointFactoryCalled = true;
                    return "fullyQualifiedNamespace";
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
    public void add_health_check_using_factories_with_options_when_properly_configured()
    {
        AzureServiceBusQueueHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ => "fullyQualifiedNamespace", _ => "queueName", _ => new AzureCliCredential(),
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
        configurationOptions.IsExceptionDetailsRequired.ShouldBeTrue();
    }
    [Fact]
    public void add_health_check_using_factories_without_giving_exception_in_result_for_unhealthy_queues()
    {
        AzureServiceBusQueueHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ => "fullyQualifiedNamespace", _ => "queueName", _ => new AzureCliCredential(),
                options =>
                {
                    configurationCalled = true;
                    configurationOptions = options;
                }, isExceptionDetailsRequired: false);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        configurationCalled.ShouldBeTrue();
        configurationOptions.ShouldNotBeNull();
        configurationOptions.UsePeekMode.ShouldBeTrue();
        configurationOptions.IsExceptionDetailsRequired.ShouldBeFalse();
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
        bool endpointFactoryCalled = false, queueNameFactoryCalled = false, tokenCredentialFactoryCalled = false;

        var services = new ServiceCollection();
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
                name: "azureservicebusqueuecheck");

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

        var exception = Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("options");
    }
}
