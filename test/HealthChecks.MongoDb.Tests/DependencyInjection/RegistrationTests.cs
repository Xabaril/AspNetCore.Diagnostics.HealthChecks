using MongoDB.Driver;

namespace HealthChecks.MongoDb.Tests.DependencyInjection;

public class mongodb_registration_should
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void add_health_check_when_properly_configured_mongoClientFactory_defaults(bool registerAsAbstraction)
    {
        var services = new ServiceCollection();

        if (registerAsAbstraction)
        {
            services.AddSingleton(sp => new MongoClient(MongoUrl.Create("mongodb://connectionstring")));
        }
        else
        {
            services.AddSingleton<IMongoClient>(sp => new MongoClient(MongoUrl.Create("mongodb://connectionstring")));
        }

        services.AddHealthChecks()
            .AddMongoDb(); // use default arguments and get the client resolved from the container

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("mongodb");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_mongoClientFactory_custom()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton(MongoClientSettings.FromUrl(MongoUrl.Create("mongodb://connectionstring")))
            .AddHealthChecks()
            .AddMongoDb(sp => new MongoClient(sp.GetRequiredService<MongoClientSettings>()));

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("mongodb");
        check.ShouldBeOfType<MongoDbHealthCheck>();
    }

    [Fact]
    public void add_health_check_when_properly_configured_mongoDatabaseNameFactory_custom()
    {
        bool called = false;
        var services = new ServiceCollection();

        services
            .AddSingleton(sp => new MongoClient(MongoUrl.Create("mongodb://connectionstring")))
            .AddHealthChecks()
            .AddMongoDb(databaseNameFactory: _ => { called = true; return "customName"; });

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("mongodb");
        check.ShouldBeOfType<MongoDbHealthCheck>();
        called.ShouldBeTrue();
    }
}
