using System.Net;

namespace HealthChecks.System.Tests.Functional;

public class file_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_file_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                   .AddFile(setup => setup.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "HealthChecks.System.Tests.dll")), tags: new string[] { "file" });
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("file")
               });
           });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_no_file_provided()
    {
        var webHostBuilder = new WebHostBuilder()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                   .AddFile(setup =>
                   {
                   }, tags: new string[] { "file" });
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("file")
               });
           });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_file_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFile(setup => setup.AddFile($"{Directory.GetCurrentDirectory()}/non-existing-file"), tags: new string[] { "file" });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("file")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
