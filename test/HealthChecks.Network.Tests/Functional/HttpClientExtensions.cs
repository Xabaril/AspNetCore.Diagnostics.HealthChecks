using System.Text.Json;
using System.Text.Json.Serialization;

namespace System.Net.Http;

public static class HttpClientExtensions
{
    public static async Task<T?> GetAsJson<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url).ConfigureAwait(false);
        string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        // UIResponseWriter durumları metin olarak yazar ("Unhealthy"); okurken de aynı dönüştürücü gerekir.
        options.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Deserialize<T>(content, options);
    }
}
