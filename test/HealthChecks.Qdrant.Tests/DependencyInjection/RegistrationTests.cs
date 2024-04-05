namespace HealthChecks.Qdrant.Tests.DependencyInjection;

public class qdrant_registration_should
{
    private const string FAKE_CONNECTION_STRING = "http://server";
    private const string DEFAULT_CHECK_NAME = "qdrant";

    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddQdrant(qdrantConnectionString: FAKE_CONNECTION_STRING);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(DEFAULT_CHECK_NAME);
        check.ShouldBeOfType<QdrantHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + DEFAULT_CHECK_NAME;

        services.AddHealthChecks()
            .AddQdrant(FAKE_CONNECTION_STRING, name: customCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<QdrantHealthCheck>();
    }

    [Fact]
    public void add_named_health_check_with_connection_string_factory_by_iServiceProvider_registered()
    {
        var services = new ServiceCollection();
        var customCheckName = "my-" + DEFAULT_CHECK_NAME;
        services.AddSingleton(new QdrantSetting
        {
            ConnectionString = FAKE_CONNECTION_STRING
        });

        services.AddHealthChecks()
            .AddQdrant((sp, options) =>
            {
                var connectionString = sp.GetRequiredService<QdrantSetting>().ConnectionString;

                options.ConnectionUri = new Uri(connectionString);
            }, name: customCheckName);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(customCheckName);
        check.ShouldBeOfType<QdrantHealthCheck>();
    }
}

public class QdrantSetting
{
    public string ConnectionString { get; set; } = null!;
}
