using System.Net;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace HealthChecks.RavenDb.Tests.Functional;

public class ravendb_healthcheck_should_single_connection_string : IClassFixture<RavenDbContainerFixture>
{
    private readonly string _connectionString;

    public ravendb_healthcheck_should_single_connection_string(RavenDbContainerFixture ravenDbFixture)
    {
        _connectionString = ravenDbFixture.GetConnectionString();

        try
        {
            using var store = new DocumentStore();

            store.Urls = [_connectionString];

            store.Initialize();

            store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord("Demo")));
        }
        catch
        {
            // ignored
        }
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddRavenDB(setup => setup.Urls = [_connectionString], tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddRavenDB(setup => setup.Urls = [_connectionString], "Demo", tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_not_available()
    {
        var connectionString = "http://localhost:9999";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddRavenDB(setup => setup.Urls = [connectionString], tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_database_doesnot_exist()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddRavenDB(setup =>
                {
                    setup.Urls = [_connectionString];
                    setup.Database = "ThisDatabaseReallyDoesnExist";
                }, "ThisDatabaseReallyDoesnExist", tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
