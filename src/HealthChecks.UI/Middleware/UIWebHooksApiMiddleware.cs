using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    public class UIWebHooksApiMiddleware
    {
        private readonly JsonSerializerSettings _jsonSerializationSettings;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UIWebHooksApiMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _jsonSerializationSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var settings = scope.ServiceProvider.GetService<IOptions<Settings>>();
                var sanitizedWebhooksResponse = settings.Value.Webhooks.Select(item => new
                {
                    item.Name,
                    Payload = string.IsNullOrEmpty(item.Payload) ? new JObject() : JObject.Parse(Regex.Unescape(item.Payload))
                });
                context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;
                var response = JsonConvert.SerializeObject(sanitizedWebhooksResponse, _jsonSerializationSettings);

                await context.Response.WriteAsync(response);
            }
        }
    }
}
