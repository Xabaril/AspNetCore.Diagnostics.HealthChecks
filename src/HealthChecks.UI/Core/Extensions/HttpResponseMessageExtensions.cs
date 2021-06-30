using Newtonsoft.Json;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<TContent> As<TContent>(this HttpResponseMessage response, params JsonConverter[] converters)
        {
            if (response != null)
            {
                var body = await response.Content
                    .ReadAsStringAsync();

                if (body != null)
                {
                    var content = JsonConvert.DeserializeObject<TContent>(body, converters);

                    if (content != null)
                    {
                        return content;
                    }
                }
            }

            throw new InvalidOperationException($"Response is null or message can't be deserialized as {typeof(TContent).FullName}.");
        }
    }
}
