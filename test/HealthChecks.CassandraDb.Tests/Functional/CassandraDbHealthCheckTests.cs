using System.Net;
using HealthChecks.CassandraDb.DependencyInjection;

namespace HealthChecks.CassandraDb.Tests.Functional;

public class CassandraDbHealthCheckTests
{
    [Fact]
    public async Task be_healthy_if_cassandra_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                        .AddCassandra(contactPoint: "cassandradb", keyspace: "system", query: "SELECT now() FROM system.local", configureClusterBuilder: builder => builder.WithPort(9042), tags: new string[] { "cassandra" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cassandra")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_cassandra_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                        .AddCassandra(contactPoint: "invalid-host", keyspace: "system", query: "SELECT now() FROM system.local", configureClusterBuilder: builder => builder.WithPort(9042), tags: new string[] { "cassandra" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cassandra")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task be_unhealthy_if_cassandra_query_is_not_valid()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                        .AddCassandra(contactPoint: "cassandradb", keyspace: "system", query: "SELECT invalid_query FROM system.local", configureClusterBuilder: builder => builder.WithPort(9042), tags: new string[] { "cassandra" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("cassandra")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

}
