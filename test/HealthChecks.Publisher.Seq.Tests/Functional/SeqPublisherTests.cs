using System.Net;

namespace HealthChecks.Publisher.Seq.Tests.Functional;

public class seq_publisher_should
{
    [Fact]
    public async Task handle_trailing_slash_in_endpoint()
    {
        var options = new SeqOptions
        {
            ApiKey = "test-key",
            Endpoint = "http://localhost:5341/"
        };

        var expectedUri = new Uri("http://localhost:5341/api/events/raw?apiKey=test-key");

        // Setup mocks
        using var handler = new MockClientHandler();
        HttpClient HttpClientFactory() => new(handler);

        var testReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        // Create publisher and publish
        var publisher = new SeqPublisher(HttpClientFactory, options);
        await publisher.PublishAsync(testReport, CancellationToken.None);

        handler.Request.ShouldNotBeNull();
        handler.Request.RequestUri.ShouldBe(expectedUri);
    }

    [Fact]
    public void throw_exception_when_endpoint_is_null()
    {
        var options = new SeqOptions
        {
            ApiKey = "test-key",
            Endpoint = null!
        };

        static HttpClient HttpClientFactory() => new();

        var ex = Should.Throw<ArgumentNullException>(() => new SeqPublisher(HttpClientFactory, options));
        ex.ParamName.ShouldBe("options.Endpoint");
    }

    [Fact]
    public async Task apply_configure_action_on_raw_events()
    {
        var options = new SeqOptions
        {
            ApiKey = "test-key",
            Endpoint = "http://localhost:5341/",
            Configure = rawEvents =>
            {
                foreach (var rawEvent in rawEvents.Events)
                {
                    rawEvent.Properties.Add("Application", "MyApplication");
                }
            }
        };

        // Setup mocks
        using var handler = new MockClientHandler();
        HttpClient HttpClientFactory() => new(handler);

        var testReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero);

        // Create publisher and publish
        var publisher = new SeqPublisher(HttpClientFactory, options);
        await publisher.PublishAsync(testReport, CancellationToken.None);

        handler.Request.ShouldNotBeNull();
        handler.Request.Content.ShouldNotBeNull();
        handler.StringContent.ShouldNotBeNull().ShouldContain("\"Application\":\"MyApplication\"");
    }

    private class MockClientHandler : HttpClientHandler
    {
        public HttpRequestMessage? Request;
        public string? StringContent;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            if (request.Content != null)
                StringContent = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            return response;
        }
    }
}
