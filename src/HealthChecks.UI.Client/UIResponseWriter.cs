using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;

namespace HealthChecks.UI.Client
{
    public static class UIResponseWriter
    {
        const string DEFAULT_CONTENT_TYPE = "application/json";
        public static Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report) => WriteHealthCheckUIResponse(httpContext, report, null);

        public static Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report, Action<JsonSerializerSettings> jsonConfigurator)
        {
            var response = "{}";

            if (report != null)
            {
                var settings = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                jsonConfigurator?.Invoke(settings);

                settings.Converters.Add(new StringEnumConverter());

                httpContext.Response.ContentType = DEFAULT_CONTENT_TYPE;

                var uiReport = UIHealthReport
                    .CreateFrom(report);

                response = JsonConvert.SerializeObject(uiReport, settings);
            }

            return httpContext.Response.WriteAsync(response);
        }
    }
}
