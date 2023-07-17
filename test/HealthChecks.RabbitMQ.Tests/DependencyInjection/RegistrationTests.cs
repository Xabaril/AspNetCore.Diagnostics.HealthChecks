namespace HealthChecks.RabbitMQ.Tests.DependencyInjection;

public class rabbitmq_registration_should
{
    private const string FAKE_CONNECTION_STRING = "amqp://server";
    private const string DEFAULT_CHECK_NAME = "rabbitmq";

    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddRabbitMQ(rabbitConnectionString: FAKE_CONNECTION_STRING);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(DEFAULT_CHECK_NAME);
        check.ShouldBeOfType<RabbitMQHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + DEFAULT_CHECK_NAME;

        services.AddHealthChecks()
            .AddRabbitMQ(FAKE_CONNECTION_STRING, name: customCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<RabbitMQHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_with_connection_string_factory_by_iServiceProvider_registered()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + DEFAULT_CHECK_NAME;
        services.AddSingleton(new RabbitMqSetting
        {
            ConnectionString = FAKE_CONNECTION_STRING
        });

        services.AddHealthChecks()
            .AddRabbitMQ(sp =>
            {
                var connectionString = sp.GetRequiredService<RabbitMqSetting>().ConnectionString;

                return new RabbitMQHealthCheckOptions
                {
                    ConnectionUri = new Uri(connectionString)
                };
            }, name: customCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<RabbitMQHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_with_uri_string_factory_by_iServiceProvider_registered()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + DEFAULT_CHECK_NAME;
        services.AddSingleton(new RabbitMqSetting
        {
            ConnectionString = FAKE_CONNECTION_STRING
        });

        services.AddHealthChecks()
            .AddRabbitMQ(sp =>
            {
                var connectionString = new Uri(sp.GetRequiredService<RabbitMqSetting>().ConnectionString);

                return new RabbitMQHealthCheckOptions
                {
                    ConnectionUri = connectionString
                };
            }, name: customCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<RabbitMQHealthCheck>();
    }
}

public class RabbitMqSetting
{
    public string ConnectionString { get; set; } = null!;
}
