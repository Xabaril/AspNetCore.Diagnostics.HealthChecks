using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Testcontainers.Elasticsearch;

namespace HealthChecks.Elasticsearch.Tests;

public class ElasticsearchContainerFixture : IAsyncLifetime
{
    private const string Registry = "docker.io";

    private const string Image = "library/elasticsearch";

    private const string Tag = "8.19.2";

    private const string ApiKeyName = "healthchecks";

    private string? _apiKey;

    public string Username => ElasticsearchBuilder.DefaultUsername;

    public string Password => ElasticsearchBuilder.DefaultPassword;

    public ElasticsearchContainer? Container { get; private set; }

    public string GetConnectionString()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return Container.GetConnectionString();
    }

    public string GetApiKey()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        return _apiKey ?? throw new InvalidOperationException("The API key was not initialized.");
    }

    public async Task InitializeAsync()
    {
        Container = await CreateContainerAsync();

        await SetupApiKeyAsync();
    }

    public Task DisposeAsync() => Container?.DisposeAsync().AsTask() ?? Task.CompletedTask;

    private static async Task<ElasticsearchContainer?> CreateContainerAsync()
    {
        var container = new ElasticsearchBuilder()
            .WithImage($"{Registry}/{Image}:{Tag}")
            .Build();

        await container.StartAsync();

        return container;
    }

    private async Task SetupApiKeyAsync()
    {
        if (Container is null)
        {
            throw new InvalidOperationException("The test container was not initialized.");
        }

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = delegate
            {
                return true;
            }
        };

        using var httpClient = new HttpClient(handler);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            nameof(AuthenticationSchemes.Basic),
            Convert.ToBase64String(Encoding.ASCII.GetBytes(
                $"{ElasticsearchBuilder.DefaultUsername}:{ElasticsearchBuilder.DefaultPassword}")));

        var uriBuilder = new UriBuilder(
            Uri.UriSchemeHttps,
            Container.Hostname,
            Container.GetMappedPublicPort(ElasticsearchBuilder.ElasticsearchHttpsPort),
            "/_security/api_key");

        using var response = await httpClient
            .PostAsJsonAsync(uriBuilder.Uri, new { name = ApiKeyName })
            .ConfigureAwait(false);

        var apiKeyResponse = await response.Content.ReadFromJsonAsync<ApiKeyResponse>().ConfigureAwait(false)
                             ?? throw new JsonException();

        _apiKey = apiKeyResponse.Encoded;
    }

    private record ApiKeyResponse(string Encoded);
}
