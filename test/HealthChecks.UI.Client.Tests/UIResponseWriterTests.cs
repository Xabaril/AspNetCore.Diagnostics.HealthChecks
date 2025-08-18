using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Http;

namespace HealthChecks.UI.Client.Tests;

public class UIResponseWriterTests
{
    [Fact]
    public async Task should_response_with_obfuscated_exception_when_exception_message_is_defined()
    {
        var healthReportKey = "Health Check with Exception";
        var exceptionMessage = "Exception Occurred.";
        var httpContext = new DefaultHttpContext();
        // create new memory stream because DefaultHttpContext has NullStream
        httpContext.Response.Body = new MemoryStream();
        var entries = new Dictionary<string, HealthReportEntry>
        {
            { healthReportKey, new HealthReportEntry(HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(1), new Exception("Custom Exception"), null) }
        };
        var report = new HealthReport(entries, TimeSpan.FromSeconds(1));

        await UIResponseWriter.WriteHealthCheckUIResponseNoExceptionDetails(httpContext, report);

        // reset pointer to the beginning
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var healthReport = await JsonSerializer.DeserializeAsync<UIHealthReport>(httpContext.Response.Body, CreateJsonOptions());

        httpContext.Response.ContentType.ShouldBe("application/json");
        healthReport.ShouldNotBeNull().Entries.ShouldHaveSingleItem();
        healthReport.ShouldNotBeNull().Entries[healthReportKey].Exception.ShouldBe(exceptionMessage);
    }

    [Theory]
    [InlineData("你好")]
    [InlineData("жарко")]
    [InlineData("ąężćś")]
    public async Task should_not_encode_unicode_characters(string healthReportKey)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var entries = new Dictionary<string, HealthReportEntry>
        {
            { healthReportKey, new HealthReportEntry(HealthStatus.Healthy, null, TimeSpan.FromSeconds(1), null, null) }
        };
        var report = new HealthReport(entries, TimeSpan.FromSeconds(1));

        await UIResponseWriter.WriteHealthCheckUIResponse(httpContext, report);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        var responseStreamReader = new StreamReader(httpContext.Response.Body);
        var responseAsText = await responseStreamReader.ReadToEndAsync();

        responseAsText.ShouldContain(healthReportKey);
    }

    [Fact]
    public async Task should_use_custom_jsonserializersettings_when_provided()
    {
        var healthReportKey = "Some key";
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var entries = new Dictionary<string, HealthReportEntry>
        {
            { healthReportKey, new HealthReportEntry(HealthStatus.Healthy, null, TimeSpan.FromSeconds(1), null, null) }
        };
        var report = new HealthReport(entries, TimeSpan.FromSeconds(1));

        var customJsonSettings = new Lazy<JsonSerializerOptions>(() => new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        var customWriter = UIResponseWriter.CreateResponseWriter(customJsonSettings);
        await customWriter(httpContext, report);

        httpContext.Response.ContentType.ShouldBe("application/json");

        // reset pointer to the beginning
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        var responseStreamReader = new StreamReader(httpContext.Response.Body);
        var responseAsText = await responseStreamReader.ReadToEndAsync();

        responseAsText.ShouldContain(healthReportKey);
        responseAsText.Split('\n').Length.ShouldBeGreaterThan(1);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
