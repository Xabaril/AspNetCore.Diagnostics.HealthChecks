using System.Net;
using MongoDB.Driver;

namespace HealthChecks.MongoDb.Tests.Functional;

public class mongodb_healthcheck_should
{
    [Fact]
    public async Task be_healthy_listing_all_databases_if_mongodb_is_available()
    {
        var connectionString = "mongodb://localhost:27017";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient(connectionString))
                    .AddHealthChecks()
                    .AddMongoDb(tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_on_specified_database_if_mongodb_is_available_and_database_exist()
    {
        var connectionString = "mongodb://localhost:27017";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient(connectionString))
                    .AddHealthChecks()
                    .AddMongoDb(databaseNameFactory: _ => "local", tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_on_connectionstring_specified_database_if_mongodb_is_available_and_database_exist()
    {
        var connectionString = "mongodb://localhost:27017/local";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient(connectionString))
                    .AddHealthChecks()
                    .AddMongoDb(tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_on_connectionstring_specified_database_if_mongodb_is_available_and_database_exist_dbFactory()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient("mongodb://localhost:27017").GetDatabase("namedDb"))
                    .AddHealthChecks()
                    .AddMongoDb(dbFactory: sp => sp.GetRequiredService<IMongoDatabase>(), tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_on_connectionstring_specified_database_if_mongodb_is_available_and_database_not_exist()
    {
        // NOTE: with mongodb the database is created automatically the first time something is written to it
        var connectionString = "mongodb://localhost:27017/nonexisting";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient(connectionString))
                    .AddHealthChecks()
                    .AddMongoDb(tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_listing_all_databases_if_mongodb_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient("mongodb://nonexistingdomain:27017"))
                    .AddHealthChecks()
                    .AddMongoDb(tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nonexistingdatabase")]
    public async Task be_unhealthy_on_specified_database_if_mongodb_is_not_available(string mongoDatabaseName)
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddSingleton(sp => new MongoClient("mongodb://nonexistingdomain:27017"))
                    .AddHealthChecks()
                    .AddMongoDb(databaseNameFactory: _ => mongoDatabaseName, tags: ["mongodb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("mongodb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
