using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HealthChecks.Elasticsearch.Tests.Fixtures;

public class ElasticContainerFixture : IAsyncLifetime
{
    private const string SETUP_DONE_MESSAGE = "All done!";
    private const long TIME_OUT_IN_MILLIS = 180000;
    private const string ELASTIC_CONTAINER_NAME = "es01";
    private const string CONTAINER_CERTIFICATE_PATH = "/usr/share/elasticsearch/config/certs/ca/ca.crt";

    public const string ELASTIC_PASSWORD = "abcDEF123!";
    private readonly string _composeFilePath = $"{Directory.GetCurrentDirectory()}/Resources/docker-compose.yml";
    public string? ApiKey;

    public async Task InitializeAsync() => ApiKey = await SetApiKeyInElasticSearchAsync().ConfigureAwait(false);

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task<string> SetApiKeyInElasticSearchAsync()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        using var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"elastic:{ELASTIC_PASSWORD}")));
        using var response = await httpClient.PostAsJsonAsync("https://localhost:9200/_security/api_key?pretty",
            new { name = "new-api-key", role_descriptors = new { } }).ConfigureAwait(false);
        var apiKeyResponse = await response.Content.ReadFromJsonAsync<ApiKeyResponse>().ConfigureAwait(false) ?? throw new JsonException();

        return apiKeyResponse.Encoded;
    }

    private record ApiKeyResponse(string Encoded);
}
