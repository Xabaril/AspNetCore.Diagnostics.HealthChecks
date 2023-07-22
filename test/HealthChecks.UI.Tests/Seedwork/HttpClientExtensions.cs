using System.Text.Json;
using System.Text.Json.Serialization;

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
            Converters =
            {
                // allowIntegerValues: true https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/1422
                new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true)
            }
        })!;
    }
}
