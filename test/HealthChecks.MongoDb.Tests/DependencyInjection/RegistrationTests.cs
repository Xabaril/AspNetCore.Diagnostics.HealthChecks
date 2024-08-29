using MongoDB.Driver;

namespace HealthChecks.MongoDb.Tests.DependencyInjection;

public class mongodb_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured_connectionString()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMongoDb("mongodb://connectionstring");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("mongodb");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }
    [Fact]
    public void add_health_check_when_properly_configured_mongoClientSettings()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMongoDb(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("mongodb");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }
    [Fact]
    public void add_health_check_when_properly_configured_mongoClientFactory()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")))
            .AddSingleton(sp => new MongoClient(sp.GetRequiredService<MongoClientSettings>()))
            .AddHealthChecks()
            .AddMongoDb(sp => sp.GetRequiredService<MongoClient>());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("mongodb");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }
    [Fact]
    public void add_named_health_check_when_properly_configured_connectionString()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddMongoDb("mongodb://connectionstring", name: "my-mongodb-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-mongodb-group");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }
    [Fact]
    public void add_named_health_check_when_properly_configured_mongoClientSettings()
    {
        var services = new ServiceCollection();

        services
            .AddHealthChecks()
            .AddMongoDb(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")), name: "my-mongodb-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-mongodb-group");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }
    [Fact]
    public void add_named_health_check_when_properly_configured_mongoClientFactory()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")))
            .AddSingleton(sp => new MongoClient(sp.GetRequiredService<MongoClientSettings>()))
            .AddHealthChecks()
            .AddMongoDb(sp => sp.GetRequiredService<MongoClient>(), name: "my-mongodb-group");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-mongodb-group");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }
}
