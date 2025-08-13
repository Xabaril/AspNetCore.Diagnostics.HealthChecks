using System.Net;
using HealthChecks.UI.Client;
using Oracle.ManagedDataAccess.Client;

namespace HealthChecks.Oracle.Tests.Functional;

public class oracle_healthcheck_should(OracleContainerFixture oracleFixture) : IClassFixture<OracleContainerFixture>
{
    [Fact]
    public async Task be_healthy_when_oracle_is_available()
    {
        string connectionString = oracleFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddOracle(connectionString, tags: ["oracle"]);
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
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_when_oracle_is_not_available()
    {
        var connectionStringBuilder = oracleFixture.GetConnectionStringBuilder();

        connectionStringBuilder.DataSource = "invalid";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddOracle(connectionStringBuilder.ConnectionString, tags: ["oracle"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_when_sql_query_is_not_valid()
    {
        string connectionString = oracleFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                .AddOracle(connectionString, "SELECT 1 FROM InvalidDb", tags: ["oracle"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_healthy_with_connection_string_factory_when_oracle_is_available()
    {
        bool factoryCalled = false;
        string connectionString = oracleFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                .AddHealthChecks()
                .AddOracle(_ =>
                {
                    factoryCalled = true;
                    return connectionString;

                }, tags: ["oracle"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("oracle")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task be_healthy_with_connection_string_and_credential_when_oracle_is_available()
    {
        bool factoryCalled = false;
        var connectionStringBuilder = oracleFixture.GetConnectionStringBuilder();
        var password = new NetworkCredential(connectionStringBuilder.UserID, connectionStringBuilder.Password).SecurePassword;
        password.MakeReadOnly();
        var credential = new OracleCredential("system", password);

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddOracle($"DATA SOURCE={connectionStringBuilder.DataSource}", tags: ["oracle"],
                        configure: options =>
                        {
                            factoryCalled = true;
                            options.Credential = credential;
                        }
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
        using var response = await server.CreateRequest("/health").GetAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        factoryCalled.ShouldBeTrue();
    }
}
