using System.Net;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Npgsql.Tests.Functional;

public class DBConfigSetting
{
    public string ConnectionString { get; set; } = null!;
}

public class npgsql_healthcheck_should(PostgreSQLContainerFixture postgreSQLContainerFixture) : IClassFixture<PostgreSQLContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_npgsql_is_available()
    {
        var connectionString = postgreSQLContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddNpgSql(connectionString, tags: ["npgsql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sql_query_is_not_valid()
    {
        var connectionString = postgreSQLContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddNpgSql(connectionString, "SELECT 1 FROM InvalidDB", tags: ["npgsql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_npgsql_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddNpgSql("Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres", tags: ["npgsql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_npgsql_is_available_by_iServiceProvider_registered()
    {
        var connectionString = postgreSQLContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddSingleton(new DBConfigSetting
                    {
                        ConnectionString = connectionString
                    });

                    webHostBuilder.Services.AddHealthChecks()
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: ["npgsql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_npgsql_is_not_available_registered()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddSingleton(new DBConfigSetting
                    {
                        ConnectionString = "Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                    });

                    webHostBuilder.Services.AddHealthChecks()
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: ["npgsql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task unhealthy_check_log_detailed_messages()
    {
        var connectionString = postgreSQLContainerFixture.GetConnectionString();

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddLogging(b =>
                            b.ClearProviders()
                            .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>())
                        )
                    .AddHealthChecks()
                    .AddNpgSql(connectionString, "SELECT 1 FROM InvalidDB", tags: ["npgsql"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        var testLoggerProvider = (TestLoggerProvider)app.Services.GetRequiredService<ILoggerProvider>();

        testLoggerProvider.ShouldNotBeNull();
        var logger = testLoggerProvider.GetLogger("Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService");

        logger.ShouldNotBeNull();
        logger?.EventLog[0].Item2.ShouldNotContain("with message '(null)'");
    }
}
