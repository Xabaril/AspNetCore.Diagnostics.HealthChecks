using System.Net;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Npgsql.Tests.Functional;

public class DBConfigSetting
{
    public string ConnectionString { get; set; } = null!;
}

public class npgsql_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_npgsql_is_available()
    {
        var connectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddNpgSql(connectionString, tags: new string[] { "npgsql" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("npgsql")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_sql_query_is_not_valid()
    {
        var connectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddNpgSql(connectionString, "SELECT 1 FROM InvalidDB", tags: new string[] { "npgsql" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("npgsql")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_npgsql_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddNpgSql("Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres", tags: new string[] { "npgsql" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("npgsql")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_if_npgsql_is_available_by_iServiceProvider_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new DBConfigSetting
                {
                    ConnectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                });

                services.AddHealthChecks()
                        .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: new string[] { "npgsql" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("npgsql")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_npgsql_is_not_available_registered()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new DBConfigSetting
                {
                    ConnectionString = "Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                });

                services.AddHealthChecks()
                        .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: new string[] { "npgsql" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("npgsql")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task unhealthy_check_log_detailed_messages()
    {
        var connectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddLogging(b =>
                        b.ClearProviders()
                        .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>())
                    )
                .AddHealthChecks()
                .AddNpgSql(connectionString, "SELECT 1 FROM InvalidDB", tags: new string[] { "npgsql" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = r => r.Tags.Contains("npgsql")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        var testLoggerProvider = (TestLoggerProvider)server.Services.GetRequiredService<ILoggerProvider>();

        testLoggerProvider.ShouldNotBeNull();
        var logger = testLoggerProvider.GetLogger("Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService");

        logger.ShouldNotBeNull();
        logger?.EventLog[0].Item2.ShouldNotContain("with message '(null)'");
    }
}
