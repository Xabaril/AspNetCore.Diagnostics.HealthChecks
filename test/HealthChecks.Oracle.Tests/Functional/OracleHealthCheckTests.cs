using System.Net;
using System.Security;
using HealthChecks.UI.Client;
using Oracle.ManagedDataAccess.Client;

namespace HealthChecks.Oracle.Tests.Functional;

public class oracle_healthcheck_should
{
    [Fact]
    public async Task be_healthy_when_oracle_is_available()
    {
        var connectionString = "Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddOracle(connectionString, tags: new string[] { "oracle" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
    }

    [Fact]
    public async Task be_unhealthy_when_oracle_is_not_available()
    {
        var connectionString = "Data Source=255.255.255.255:1521/XEPDB1;User Id=system;Password=oracle";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddOracle(connectionString, tags: new string[] { "oracle" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_sql_query_is_not_valid()
    {
        var connectionString = "Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle";
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddOracle(connectionString, "SELECT 1 FROM InvalidDb", tags: new string[] { "oracle" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_with_connection_string_factory_when_oracle_is_available()
    {
        bool factoryCalled = false;
        string connectionString = "Data Source=localhost:1521/XEPDB1;User Id=system;Password=oracle";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddOracle(_ =>
                {
                    factoryCalled = true;
                    return connectionString;

                }, tags: new string[] { "oracle" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task be_healthy_with_connection_string_and_credential_when_oracle_is_available()
    {
        bool factoryCalled = false;
        string connectionString = "Data Source=localhost:1521/XEPDB1";
        var password = new NetworkCredential("oracle", "system").SecurePassword;
        password.MakeReadOnly();
        var credential = new OracleCredential("system", password);

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddOracle(connectionString, tags: new string[] { "oracle" },
                        configure: options => options.Credential = credential
                    );
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        factoryCalled.ShouldBeTrue();
    }
}
