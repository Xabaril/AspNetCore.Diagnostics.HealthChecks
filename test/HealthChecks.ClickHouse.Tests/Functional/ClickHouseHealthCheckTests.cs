using System.Net;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace HealthChecks.ClickHouse.Tests.Functional;

public class DBConfigSetting
{
    public string ConnectionString { get; set; } = null!;
}

public class ClickHouse_healthcheck_should(ClickHouseContainerFixture clickHouseFixture) : IClassFixture<ClickHouseContainerFixture>
{
    private readonly string _connectionString = clickHouseFixture.GetConnectionString();

    [Fact]
    public async Task be_healthy_if_ClickHouse_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddClickHouse(_ => new ClickHouseConnection(_connectionString), tags: new string[] { "ClickHouse" });
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("ClickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sql_query_is_not_valid()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddClickHouse(_ => new ClickHouseConnection(_connectionString), "SELECT 1 FROM InvalidDB", tags: new string[] { "ClickHouse" });
            })
            .Configure(static app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = static r => r.Tags.Contains("ClickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_ClickHouse_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddClickHouse(static _ => new ClickHouseConnection("Host=200.0.0.1;Port=8123;Database=default;Username=default;Password=Password12!;"),
                        tags: ["ClickHouse"],
                        timeout: TimeSpan.FromSeconds(15));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ClickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_ClickHouse_is_available_by_iServiceProvider_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new DBConfigSetting
                {
                    ConnectionString = _connectionString
                });

                services.AddHealthChecks()
                        .AddClickHouse(static sp => new ClickHouseConnection(sp.GetRequiredService<DBConfigSetting>().ConnectionString), tags: new string[] { "ClickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ClickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_ClickHouse_is_not_available_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new DBConfigSetting
                {
                    ConnectionString = "Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                });

                services
                    .AddHealthChecks()
                    .AddClickHouse(
                        static sp => new ClickHouseConnection(sp.GetRequiredService<DBConfigSetting>().ConnectionString),
                        tags: ["ClickHouse"],
                        timeout: TimeSpan.FromSeconds(15));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ClickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task unhealthy_check_log_detailed_messages()
    {
        const string connectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddLogging(b =>
                        b.ClearProviders()
                        .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>())
                    )
                .AddHealthChecks()
                .AddClickHouse(static _ => new ClickHouseConnection(connectionString), "SELECT 1 FROM InvalidDB", tags: new string[] { "ClickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("ClickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        var testLoggerProvider = (TestLoggerProvider)server.Services.GetRequiredService<ILoggerProvider>();

        testLoggerProvider.ShouldNotBeNull();
        var logger = testLoggerProvider.GetLogger("Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService");

        logger.ShouldNotBeNull();
        logger?.EventLog[0].Item2.ShouldNotContain("with message '(null)'");
    }
}
