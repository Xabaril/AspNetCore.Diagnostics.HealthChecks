using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthChecks.UI.Client
{
    public static class UIResponseWriter
    {
        const string DEFAULT_CONTENT_TYPE = "application/json";

        public static Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report) => WriteHealthCheckUIResponse(httpContext, report, null);

        public static Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report, Action<JsonSerializerOptions> jsonConfigurator)
        {
            var response = "{}";

            if (report != null)
            {
                var settings = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                };

                jsonConfigurator?.Invoke(settings);
                
                settings.Converters.Add(new JsonStringEnumConverter());

                //for compatibility with older UI versions ( <3.0 ) we arrange
                //timepan serialization as s
                settings.Converters.Add(new TimeSpanConverter());

                httpContext.Response.ContentType = DEFAULT_CONTENT_TYPE;

                var uiReport = UIHealthReport
                    .CreateFrom(report);

                response = JsonSerializer.Serialize(uiReport, settings);
            }

            return httpContext.Response.WriteAsync(response);
        }
    }

    internal class TimeSpanConverter
        : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
