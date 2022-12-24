using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<T?> GetAsJson<T>(this HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
        }
    }
}
