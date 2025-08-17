
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace HealthChecks.UI.Tests;

public class ui_webhooks_api
{
    [Fact]
    public async Task should_generate_successful_response_for_webhooks_path()
    {
        // Arrange
        string webhookName = "testWebhook";
        string webhookUri = "http://webhook/sample";
        string webhookPayload = "{\"key\":\"value\"}";
        string webhookRestorePayload = "{\"key\":\"restoreValue\"}";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecksUI(setup =>
                    {
                        setup
                            .AddWebhookNotification(
                                name: webhookName,
                                uri: webhookUri,
                                payload: webhookPayload,
                                restorePayload: webhookRestorePayload);
                    })
                    .AddInMemoryStorage(databaseName: "WebhooksResponseTest");
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecksUI(); // Registers the HealthChecks UI middleware
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest(new Configuration.Options().WebhookPath).GetAsync();

        // Assert
        response.EnsureSuccessStatusCode(); // Verify 200 OK response
        response.Content.Headers.ContentType?.ToString().ShouldBe("application/json; charset=utf-8");

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<List<WebHookApiItem>>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        responseJson.ShouldNotBeNull();
        responseJson.Count.ShouldBe(1);

        var webhookResponse = responseJson.First();
        webhookResponse.Name.ShouldBe(webhookName);

        // Verify dynamic payload as serialized JSON object
        var payloadJson = JsonSerializer.Serialize(webhookResponse.Payload);
        payloadJson.ShouldBe(webhookPayload);
    }

    [Fact]
    public async Task should_generate_successful_response_for_webhooks_path_with_newline_in_payload()
    {
        // Arrange
        string webhookName = "testWebhook";
        string webhookUri = "http://webhook/sample";
        string webhookPayload = "{\"key\":\"line 1\\nline 2\"}";
        string webhookRestorePayload = "{\"key\":\"restoreValue\"}";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecksUI(setup =>
                    {
                        setup
                            .AddWebhookNotification(
                                name: webhookName,
                                uri: webhookUri,
                                payload: webhookPayload,
                                restorePayload: webhookRestorePayload);
                    })
                    .AddInMemoryStorage(databaseName: "WebhooksResponseTest");
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecksUI(); // Registers the HealthChecks UI middleware
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest(new Configuration.Options().WebhookPath).GetAsync();

        // Assert
        response.EnsureSuccessStatusCode(); // Verify 200 OK response
        response.Content.Headers.ContentType?.ToString().ShouldBe("application/json; charset=utf-8");

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<List<WebHookApiItem>>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        responseJson.ShouldNotBeNull();
        responseJson.Count.ShouldBe(1);

        var webhookResponse = responseJson.First();
        webhookResponse.Name.ShouldBe(webhookName);

        // Verify dynamic payload as serialized JSON object
        var payloadJson = JsonSerializer.Serialize(webhookResponse.Payload);
        payloadJson.ShouldBe(webhookPayload);
    }

    [Fact]
    public async Task should_generate_successful_response_for_webhooks_path_with_teams_example()
    {
        // Arrange
        string webhookName = "Teams";
        string webhookUri = "https://outlook.office.com/webhook/...";
        string webhookPayload = "{\\r\\n  \"@context\": \"http://schema.org/extensions\",\\r\\n  \"@type\": \"MessageCard\",\\r\\n  \"themeColor\": \"0072C6\",\\r\\n  \"title\": \"[[LIVENESS]] has failed!\",\\r\\n  \"text\": \"[[FAILURE]] Click **Learn More** to go to BeatPulseUI Portal\",\\r\\n  \"potentialAction\": [\\r\\n    {\\r\\n      \"@type\": \"OpenUri\",\\r\\n      \"name\": \"Lear More\",\\r\\n      \"targets\": [\\r\\n        { \"os\": \"default\", \"uri\": \"http://localhost:52665/beatpulse-ui\" }\\r\\n      ]\\r\\n    }\\r\\n  ]\\r\\n}";
        string webhookRestorePayload = "{\"text\":\"The HealthCheck [[LIVENESS]] is recovered. All is up and running\",\"channel\":\"#general\",\"link_names\": 1,\"username\":\"monkey-bot\",\"icon_emoji\":\":monkey_face\" }";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecksUI(setup =>
                    {
                        setup
                            .AddWebhookNotification(
                                name: webhookName,
                                uri: webhookUri,
                                payload: webhookPayload,
                                restorePayload: webhookRestorePayload);
                    })
                    .AddInMemoryStorage(databaseName: "WebhooksResponseTest");
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecksUI(); // Registers the HealthChecks UI middleware
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest(new Configuration.Options().WebhookPath).GetAsync();

        // Assert
        response.EnsureSuccessStatusCode(); // Verify 200 OK response
        response.Content.Headers.ContentType?.ToString().ShouldBe("application/json; charset=utf-8");

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JsonSerializer.Deserialize<List<WebHookApiItem>>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        responseJson.ShouldNotBeNull();
        responseJson.Count.ShouldBe(1);

        var webhookResponse = responseJson.First();
        webhookResponse.Name.ShouldBe(webhookName);

        // Unescape newlines, which are all outside of JSON values
        var expectedPayload = JsonSerializer.Serialize(JsonNode.Parse(Regex.Unescape(webhookPayload)));
        var payloadJson = JsonSerializer.Serialize(webhookResponse.Payload);
        payloadJson.ShouldBe(expectedPayload);
    }

    private class WebHookApiItem
    {
        public string? Name { get; set; }
        public JsonNode? Payload { get; set; }
    }
}
