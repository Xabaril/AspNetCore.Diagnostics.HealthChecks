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

        HttpClient HttpClientFactory() => new();

        var ex = Should.Throw<ArgumentNullException>(() => new SeqPublisher(HttpClientFactory, options));
        ex.ParamName.ShouldBe("options.Endpoint");
    }

    private class MockClientHandler : HttpClientHandler
    {
        public HttpRequestMessage? Request;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            };

            return Task.FromResult(response);
        }
    }
}
