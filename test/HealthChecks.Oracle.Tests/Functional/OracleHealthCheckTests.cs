using System.Net;
using HealthChecks.UI.Client;
using Oracle.ManagedDataAccess.Client;

namespace HealthChecks.Oracle.Tests.Functional;

public class oracle_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_oracle_is_available()
    {
        var connectionString = "Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddOracle(connectionString, tags: ["oracle"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oracle"),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_when_oracle_is_not_available()
    {
        var connectionString = "Data Source=255.255.255.255:1521/XEPDB1;User Id=system;Password=oracle";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddOracle(connectionString, tags: ["oracle"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_sql_query_is_not_valid()
    {
        var connectionString = "Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle";
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                    .AddOracle(connectionString, "SELECT 1 FROM InvalidDb", tags: ["oracle"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_with_connection_string_factory_when_oracle_is_available()
    {
        bool factoryCalled = false;
        string connectionString = "Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                    .AddHealthChecks()
                    .AddOracle(_ =>
                    {
                        factoryCalled = true;
                        return connectionString;

                    }, tags: ["oracle"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task be_healthy_with_connection_string_and_credential_when_oracle_is_available()
    {
        bool factoryCalled = false;
        string connectionString = "Data Source=localhost:1521/XEPDB1";
        var password = new NetworkCredential("system", "oracle").SecurePassword;
        password.MakeReadOnly();
        var credential = new OracleCredential("system", password);

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services
                        .AddHealthChecks()
                        .AddOracle(connectionString, tags: ["oracle"],
                            configure: options =>
                            {
                                factoryCalled = true;
                                options.Credential = credential;
                            }
                        );
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("oracle")
                    });
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync("/health");
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }
}
