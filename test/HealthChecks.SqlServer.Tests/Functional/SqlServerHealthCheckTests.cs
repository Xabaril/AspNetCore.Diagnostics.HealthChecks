using System.Net;

namespace HealthChecks.SqlServer.Tests.Functional;

public class sqlserver_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_sqlServer_is_available()
    {
        var connectionString = "Server=tcp:localhost,5433;Initial Catalog=master;User Id=sa;Password=Password12!;Encrypt=false";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSqlServer(connectionString, tags: new string[] { "sqlserver" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqlserver")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sqlServer_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSqlServer("Server=tcp:200.0.0.100,1833;Initial Catalog=master;User Id=sa;Password=Password12!;Encrypt=false", tags: new string[] { "sqlserver" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqlserver")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_sqlquery_spec_is_not_valid()
    {
        var connectionString = "Server=tcp:localhost,5433;Initial Catalog=master;User Id=sa;Password=Password12!;Encrypt=false";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddSqlServer(connectionString, healthQuery: "SELECT 1 FROM [NOT_VALID_DB]", tags: new string[] { "sqlserver" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("sqlserver")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
