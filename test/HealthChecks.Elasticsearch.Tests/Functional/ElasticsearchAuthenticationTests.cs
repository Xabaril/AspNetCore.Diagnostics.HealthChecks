using System.Net;
using HealthChecks.Elasticsearch.Tests.Fixtures;

namespace HealthChecks.Elasticsearch.Tests.Functional;

public class ElasticsearchAuthenticationTests : IClassFixture<ElasticContainerFixture>
{
    private readonly ElasticContainerFixture _fixture;

    public ElasticsearchAuthenticationTests(ElasticContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task be_healthy_if_elasticsearch_is_using_valid_api_key()
    {
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseApiKey(_fixture.ApiKey!);
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: new[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health",
                    new HealthCheckOptions { Predicate = r => r.Tags.Contains("elasticsearch") });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health")
            .GetAsync();

        response.StatusCode
            .ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_elasticsearch_is_using_valid_user_and_password()
    {
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseBasicAuthentication("elastic", ElasticContainerFixture.ELASTIC_PASSWORD);
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: new[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health",
                    new HealthCheckOptions { Predicate = r => r.Tags.Contains("elasticsearch") });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health")
            .GetAsync();

        response.StatusCode
            .ShouldBe(HttpStatusCode.OK);
    }
}
