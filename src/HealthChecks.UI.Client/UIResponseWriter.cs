using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.Client
{
    public static class UIResponseWriter
    {
        private const string DEFAULT_CONTENT_TYPE = "application/json";

        private static readonly byte[] _emptyResponse = new byte[] { (byte)'{', (byte)'}' };
        private static readonly Lazy<JsonSerializerOptions> _options = new(() => CreateJsonOptions());

#pragma warning disable IDE1006 // Naming Styles
        public static async Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (report != null)
            {
                httpContext.Response.ContentType = DEFAULT_CONTENT_TYPE;

                var uiReport = UIHealthReport
                    .CreateFrom(report);

                using var responseStream = new MemoryStream();

                await JsonSerializer.SerializeAsync(responseStream, uiReport, _options.Value);
                await httpContext.Response.BodyWriter.WriteAsync(responseStream.ToArray());
            }
            else
            {
                await httpContext.Response.BodyWriter.WriteAsync(_emptyResponse);
            }
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            //for compatibility with older UI versions ( <3.0 ) we arrange
            //timespan serialization as s
            options.Converters.Add(new TimeSpanConverter());

            return options;
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
