using System.Net;
using HealthChecks.Hazelcast.Tests.Fixtures;

namespace HealthChecks.Hazelcast.Tests.Functional;

public class HazelcastHealthCheckTests : IClassFixture<HazelcastFixture>
{
    private readonly HazelcastFixture _fixture;

    public HazelcastHealthCheckTests(HazelcastFixture fixture)
    {
        _fixture = fixture;
        Task.Run(() => _fixture.InitializeAsync()).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task HazelcastHealthCheck_ShouldBeHealthy()
    {
        var webHostBuilder = new WebHostBuilder()
       .ConfigureServices(services =>
       {
           services.AddHealthChecks()
            .AddHazelcast(op =>
            {
                op.Port = 5701;
                op.Server = "localhost";
                op.ClusterNames = new() { "dev" };
            }, "hazelcast");
       })
       .Configure(app =>
       {
           app.UseHealthChecks("/health", new HealthCheckOptions
           {
               Predicate = r => r.Tags.Contains("vault")
           });
       });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

}
