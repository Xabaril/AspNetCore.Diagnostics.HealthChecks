using System.Net;
using MySqlConnector;

namespace HealthChecks.MySql.Tests.Functional;

public class mysql_healthcheck_should(MySqlContainerFixture mySqlContainerFixture) : IClassFixture<MySqlContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_mysql_server_is_available_using_data_source()
    {
        var connectionString = mySqlContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddMySqlDataSource(connectionString)
                        .AddHealthChecks().AddMySql(tags: new string[] { "mysql" });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("mysql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_when_mysql_server_is_available_using_connection_string()
    {
        var connectionString = mySqlContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddMySql(connectionString, tags: ["mysql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("mysql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_when_mysql_server_is_unavailable()
    {
        var connectionString = "server=255.255.255.255;port=3306;database=information_schema;uid=root;password=Password12!";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddMySql(connectionString, tags: ["mysql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("mysql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_mysql_server_is_unavailable_using_options()
    {
        var connectionString = "server=255.255.255.255;port=3306;database=information_schema;uid=root;password=Password12!";

        var webHostBuilder = WebApplication.CreateBuilder();
                    var mysqlOptions = new MySqlHealthCheckOptions(connectionString);
                    webHostBuilder.Services.AddHealthChecks()
                        .AddMySql(mysqlOptions, tags: ["mysql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("mysql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
