using HealthChecks.AzureServiceBus.Configuration;

namespace HealthChecks.AzureServiceBus.Tests;

public class azure_service_bus_queue_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("cnn", "queueName");

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
            .AddAzureServiceBusQueue("cnn", "queueName",
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
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue("cnn", "queueName", name: "azureservicebusqueuecheck");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azureservicebusqueuecheck");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
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

        var exception = Should.Throw<ArgumentException>(() => registration.Factory(serviceProvider));
        exception.ParamName.ShouldBe("options");
    }

    [Fact]
    public void add_health_check_using_factories_when_properly_configured()
    {
        bool connectionStringFactoryCalled = false, queueNameFactoryCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ =>
                {
                    connectionStringFactoryCalled = true;
                    return "cnn";
                },
                _ =>
                {
                    queueNameFactoryCalled = true;
                    return "queueName";
                });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("azurequeue");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
        connectionStringFactoryCalled.ShouldBeTrue();
        queueNameFactoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void add_health_check_using_factories_with_options_when_properly_configured()
    {
        AzureServiceBusQueueHealthCheckOptions? configurationOptions = null;
        bool configurationCalled = false;

        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueue(_ => "cnn", _ => "queueName",
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
    }

    [Theory]
    [InlineData(null)]
    [InlineData("azureservicebusqueuecheck")]
    public void add_health_check_with_namespace_when_properly_configured(string? name)
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddAzureServiceBusQueueWithNamespace("cnn", "queueName", name);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(name ?? "azurequeue");
        check.ShouldBeOfType<AzureServiceBusQueueHealthCheck>();
    }
}
