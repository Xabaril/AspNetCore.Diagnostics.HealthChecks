using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.Client;

public static class UIResponseWriter
{
    private const string DEFAULT_CONTENT_TYPE = "application/json";

    private static readonly byte[] _emptyResponse = new byte[] { (byte)'{', (byte)'}' };
    private static readonly Lazy<JsonSerializerOptions> _options = new(CreateJsonOptions);

    private static async Task WriteHealthCheckUIResponseAsync(HttpContext httpContext, HealthReport report, Lazy<JsonSerializerOptions> jsonOptions, Func<Exception, string>? exceptionFormatter = null)
    {
        if (report != null)
        {
            httpContext.Response.ContentType = DEFAULT_CONTENT_TYPE;

            var uiReport = UIHealthReport.CreateFrom(report, exceptionFormatter);

            await JsonSerializer.SerializeAsync(httpContext.Response.Body, uiReport, jsonOptions.Value).ConfigureAwait(false);
        }
        else
        {
            await httpContext.Response.BodyWriter.WriteAsync(_emptyResponse).ConfigureAwait(false);
        }
    }
    public static Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report) // TODO: rename public API
    {
        return WriteHealthCheckUIResponseAsync(httpContext, report, _options);
    }

    public static Task WriteHealthCheckUIResponseNoExceptionDetails(HttpContext httpContext, HealthReport report)
    {
        return WriteHealthCheckUIResponseAsync(httpContext, report, _options, _ => "Exception Occurred.");
    }

    public static Func<HttpContext, HealthReport, Task> CreateResponseWriter(Lazy<JsonSerializerOptions> jsonOptions, Func<Exception, string>? exceptionFormatter = null)
    {
        return (HttpContext httpContext, HealthReport report) => WriteHealthCheckUIResponseAsync(httpContext, report, jsonOptions, exceptionFormatter);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());

        // for compatibility with older UI versions ( <3.0 ) we arrange
        // timespan serialization as s
        options.Converters.Add(new TimeSpanConverter());

        return options;
    }
}

internal class TimeSpanConverter : JsonConverter<TimeSpan>
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
