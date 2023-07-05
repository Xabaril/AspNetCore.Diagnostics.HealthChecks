using System.Text.Json;

namespace HealthChecks.UI.Tests;

public static class HttpClientExtensions
{
    public static async Task<T> GetAsJson<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url).ConfigureAwait(false);
        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        })!;
    }
}
