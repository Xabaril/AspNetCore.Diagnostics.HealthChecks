using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services.Extensions;
using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.Elasticsearch.Tests.Functional;

public class ElasticsearchAuthenticationTests
{
    private readonly string _composeFilePath = $"{Directory.GetCurrentDirectory()}\\Resources\\docker-compose.yml";
    private const string ELASTIC_PASSWORD = "abcDEF123!";
    private const string SETUP_DONE_MESSAGE = "All done!";
    private const long TIME_OUT_IN_MILLIS = 180000;
    private const string ELASTIC_CONTAINER_NAME = "es01";
    private const string CONTAINER_CERTIFICATE_PATH = "/usr/share/elasticsearch/config/certs/ca/ca.crt";

    public ElasticsearchAuthenticationTests()
    {
        var compositeService = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(_composeFilePath)
            .ForceRecreate()
            .Build()
            .Start();

        var elasticContainer = compositeService.Containers.First(container => container.Name == ELASTIC_CONTAINER_NAME);
        var setupContainer = compositeService.Containers.First(container => container != elasticContainer);
        setupContainer.WaitForMessageInLogs(SETUP_DONE_MESSAGE, TIME_OUT_IN_MILLIS);
        elasticContainer.CopyFrom(CONTAINER_CERTIFICATE_PATH, ".", true);
    }

    [Fact]
    public async Task be_healthy_if_elasticsearch_is_using_valid_api_key()
    {
        var apiKey = await SetApiKeyInElasticSearchAsync();
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseApiKey(new ApiKeyAuthenticationCredentials(apiKey));
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: new[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health")
            .GetAsync();

        response.StatusCode
            .Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_elasticsearch_is_using_invalid_api_key()
    {
        var apiKey = await SetApiKeyInElasticSearchAsync();
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseApiKey(new ApiKeyAuthenticationCredentials(apiKey + "wrongAuthKey"));
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: new[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health")
            .GetAsync();

        response.StatusCode
            .Should().Be(HttpStatusCode.ServiceUnavailable);
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
                        options.UseBasicAuthentication("elastic", ELASTIC_PASSWORD);
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: new[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health")
            .GetAsync();

        response.StatusCode
            .Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_elasticsearch_is_using_invalid_user_and_password()
    {
        var connectionString = @"https://localhost:9200";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddElasticsearch(options =>
                    {
                        options.UseServer(connectionString);
                        options.UseBasicAuthentication("elastic", ELASTIC_PASSWORD + "wrongPassword");
                        options.UseCertificateValidationCallback(delegate
                        {
                            return true;
                        });
                        options.RequestTimeout = TimeSpan.FromSeconds(30);
                    }, tags: new[] { "elasticsearch" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("elasticsearch")
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest($"/health")
            .GetAsync();

        response.StatusCode
            .Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    private async Task<string> SetApiKeyInElasticSearchAsync()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = delegate
        {
            return true;
        };
        using var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"elastic:{ELASTIC_PASSWORD}")));
        using var response = await httpClient.PostAsJsonAsync("https://localhost:9200/_security/api_key?pretty", new
        {
            name = "new-api-key",
            role_descriptors = new {}
        });
        var apiKeyResponse = await response.Content.ReadFromJsonAsync<ApiKeyResponse>() ?? throw new JsonException();

        return apiKeyResponse.Encoded;
    }

    private record ApiKeyResponse(string Encoded);
}
