using System.Net;
using HealthChecks.UI.Client;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace HealthChecks.RavenDb.Tests.Functional;

public class ravendb_healthcheck_should
{
    private readonly string[] _urls = new[] { "http://localhost:9030" };

    public ravendb_healthcheck_should()
    {
        try
        {
            using var store = new DocumentStore
            {
                Urls = _urls,
            };

            store.Initialize();

            store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord("Demo")));
        }
        catch { }
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ => _.Urls = _urls, tags: new string[] { "ravendb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "Demo";
                    }, tags: new string[] { "ravendb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_timeout_is_too_low()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "Demo";
                        _.RequestTimeout = TimeSpan.FromMilliseconds(100);
                    }, tags: new string[] { "ravendb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
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
                    .AddRavenDB(_ => _.Urls = new string[] { connectionString }, tags: new string[] { "ravendb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_database_doesnot_exist()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "ThisDatabaseReallyDoesnExist";
                    }, tags: new string[] { "ravendb" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }
}
