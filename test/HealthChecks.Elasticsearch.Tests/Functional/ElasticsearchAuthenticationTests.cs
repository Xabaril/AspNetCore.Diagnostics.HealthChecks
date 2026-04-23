using System.Net;

namespace HealthChecks.Elasticsearch.Tests.Functional;

public class ElasticsearchAuthenticationTests(ElasticsearchContainerFixture elasticsearchFixture) : IClassFixture<ElasticsearchContainerFixture>
{
    [Fact]
    public async Task be_healthy_if_elasticsearch_is_using_valid_api_key()
    {
        string connectionString = elasticsearchFixture.GetConnectionString();
        string apiKey = elasticsearchFixture.GetApiKey();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseApiKey(apiKey);
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: ["elasticsearch"]);
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
        string connectionString = elasticsearchFixture.GetConnectionString();

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseBasicAuthentication(elasticsearchFixture.Username, elasticsearchFixture.Password);
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: ["elasticsearch"]);
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
