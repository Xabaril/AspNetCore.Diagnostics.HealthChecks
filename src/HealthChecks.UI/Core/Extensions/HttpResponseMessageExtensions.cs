using Newtonsoft.Json;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<TContent> As<TContent>(this HttpResponseMessage response)
        {
            if (response != null)
            {
                var content = await response.Content
                    .ReadAsStringAsync();

                if (content != null)
                {
                    return JsonConvert.DeserializeObject<TContent>(content);
                }
            }
            throw new InvalidOperationException($"Response is null or message can't be deserialized as {typeof(TContent).FullName}.");
        }
    }
}
