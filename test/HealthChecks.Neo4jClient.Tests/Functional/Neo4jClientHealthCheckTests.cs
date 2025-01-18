using System.Net;
using Neo4jClient;

namespace HealthChecks.Neo4jClient.Tests.Functional;

public class Neo4jClientHealthCheckTests
{
    [Fact]
    public async Task be_unhealthy_when_username_not_right_with_503_status_code_response_from_test_server()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var graphClient = new BoltGraphClient("bolt://localhost:7687", "neo4j_should_be_not_right_name", "neo4j");
                services.AddSingleton<IGraphClient>(graphClient);

                services.AddHealthChecks()
                    .AddNeo4jClient(_ => _.GetRequiredService<IGraphClient>(), tags: ["neo4j"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("neo4j")
                });
            });

        using var server = new TestServer(webHostBuilder);
        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
