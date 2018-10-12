using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<TContent> As<TContent>(this HttpResponseMessage response)
        {
            var content = await response.Content
                .ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TContent>(content);
        }
    }
}
