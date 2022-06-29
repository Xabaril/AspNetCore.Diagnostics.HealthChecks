using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HealthChecks.UI.Middleware
{
    internal class UISettingsMiddleware
    {
        private readonly JsonSerializerSettings _jsonSerializationSettings;
        private readonly object _uiOutputSettings;

        public UISettingsMiddleware(RequestDelegate next, IOptions<Settings> settings)
        {
            _ = next;
            _ = settings ?? throw new ArgumentNullException(nameof(settings));
            _uiOutputSettings = new
            {
                PollingInterval = settings.Value.EvaluationTimeInSeconds,
                HeaderText = settings.Value.HeaderText
            };

            _jsonSerializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //TODO: switch to STJ and write directly into response body
            string content = JsonConvert.SerializeObject(_uiOutputSettings, _jsonSerializationSettings);
            context.Response.ContentType = Keys.DEFAULT_RESPONSE_CONTENT_TYPE;

            await context.Response.WriteAsync(content);
        }
    }
}
