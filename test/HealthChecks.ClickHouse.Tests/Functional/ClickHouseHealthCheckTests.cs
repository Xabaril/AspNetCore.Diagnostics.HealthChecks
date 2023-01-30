using System.Net;

namespace HealthChecks.ClickHouse.Tests.Functional;

public class clickHouse_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_clickhouse_is_available()
    {
        var connectionString = "Host=127.0.0.1;Port=8123;Username=default;Password=;Database=default";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddClickHouse(connectionString, tags: new string[] { "clickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("clickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sql_query_is_not_valid()
    {
        var connectionString = "Host=127.0.0.1;Port=8123;Username=default;Password=;Database=default";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddClickHouse(connectionString, "SELECT 1 FROM InvalidDB", tags: new string[] { "clickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("clickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_clickHouse_is_not_available()
    {
        var connectionString = "Host=200.0.0.1;Port=8123;Username=default;Password=;Database=default";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddClickHouse(connectionString, tags: new string[] { "clickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("clickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_clickHouse_is_available_by_iServiceProvider_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new DBConfigSetting
                {
                    ConnectionString = "Host=127.0.0.1;Port=8123;Username=default;Password=;Database=default"
                });

                services.AddHealthChecks()
                    .AddClickHouse(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: new string[] { "clickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("clickHouse")
                });
            });


        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_clickHouse_is_not_available_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new DBConfigSetting
                {
                    ConnectionString = "Host=200.0.0.1;Port=8123;Username=default;Password=;Database=default"
                });

                services.AddHealthChecks()
                    .AddClickHouse(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: new string[] { "clickHouse" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("clickHouse")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}

public class DBConfigSetting
{
    public string ConnectionString { get; set; } = null!;
}
