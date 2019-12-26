using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace HealthChecks.UI.Client
{
    public static class UIResponseWriter
    {
        private static byte[] emptyResponse = new byte[] { (byte)'{', (byte)'}' };
        const string DEFAULT_CONTENT_TYPE = "application/json";

        public static async Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report) => await WriteHealthCheckUIResponse(httpContext, report, null);

        public static async Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report, Action<JsonSerializerOptions> jsonConfigurator)
        {
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

                using var responseStream = new MemoryStream();

                await JsonSerializer.SerializeAsync(responseStream, uiReport, settings);
                await httpContext.Response.BodyWriter.WriteAsync(responseStream.ToArray());
            }
            else
            {
                await httpContext.Response.BodyWriter.WriteAsync(emptyResponse);
            }

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
