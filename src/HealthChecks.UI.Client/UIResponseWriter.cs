using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace HealthChecks.UI.Client
{
    public static class UIResponseWriter
    {
        public static Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport result)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            settings.Converters.Add(new StringEnumConverter());

            httpContext.Response.ContentType = "application/json";

            var report = UIHealthReport.CreateFrom(result);
            
            var jsonResponse = result != null ?
                JsonConvert.SerializeObject(report, settings)
                : "{}";

            return httpContext.Response.WriteAsync(jsonResponse);
        }
    }
}
