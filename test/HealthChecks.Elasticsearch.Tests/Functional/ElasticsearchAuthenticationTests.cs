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

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddElasticsearch(options =>
                        {
                            options.UseServer(connectionString);
                            options.UseApiKey(_fixture.ApiKey!);
                            options.UseCertificateValidationCallback(delegate
                            {
                                return true;
                            });
                            options.RequestTimeout = TimeSpan.FromSeconds(30);
                        }, tags: ["elasticsearch"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health",
                        new HealthCheckOptions { Predicate = r => r.Tags.Contains("elasticsearch") });
        await app.StartAsync();


        var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode
            .ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_elasticsearch_is_using_valid_user_and_password()
    {
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddElasticsearch(options =>
                        {
                            options.UseServer(connectionString);
                            options.UseBasicAuthentication("elastic", ElasticContainerFixture.ELASTIC_PASSWORD);
                            options.UseCertificateValidationCallback(delegate
                            {
                                return true;
                            });
                            options.RequestTimeout = TimeSpan.FromSeconds(30);
                        }, tags: ["elasticsearch"]);
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health",
                        new HealthCheckOptions { Predicate = r => r.Tags.Contains("elasticsearch") });
        await app.StartAsync();


        var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode
            .ShouldBe(HttpStatusCode.OK);
    }
}
