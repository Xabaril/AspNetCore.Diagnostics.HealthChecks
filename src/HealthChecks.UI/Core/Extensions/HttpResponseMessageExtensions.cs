using System.Threading.Tasks;
using Newtonsoft.Json;

namespace System.Net.Http
{
    public static class HttpResponseMessageExtensions
    {
#pragma warning disable IDE1006 // Naming Styles
        public static async Task<TContent> As<TContent>(this HttpResponseMessage response)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (response != null)
            {
                var body = await response.Content
                    .ReadAsStringAsync();

                if (body != null)
                {
                    var content = JsonConvert.DeserializeObject<TContent>(body);

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
