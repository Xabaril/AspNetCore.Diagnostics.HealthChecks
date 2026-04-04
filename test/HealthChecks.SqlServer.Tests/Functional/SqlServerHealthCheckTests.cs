using System.Net;

namespace HealthChecks.SqlServer.Tests.Functional;

public class sqlserver_healthcheck_should(SqlServerContainerFixture sqlServerContainerFixture) : IClassFixture<SqlServerContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_sqlServer_is_available()
    {
        var connectionString = sqlServerContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSqlServer(connectionString, tags: ["sqlserver"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sqlserver")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sqlServer_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSqlServer("Server=tcp:localhost,1833;Initial Catalog=master;User Id=sa;Password=Password12!;Encrypt=false;Connection Timeout=10", tags: ["sqlserver"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sqlserver")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_sqlquery_spec_is_not_valid()
    {
        var connectionString = sqlServerContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddSqlServer(connectionString, healthQuery: "SELECT 1 FROM [NOT_VALID_DB]", tags: ["sqlserver"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("sqlserver")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
