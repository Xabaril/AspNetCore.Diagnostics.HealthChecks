using System.Net;

namespace HealthChecks.System.Tests.Functional;

public class folder_healthcheck_should
{
    [Fact]
    public async Task be_healthy_if_folder_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                   .AddFolder(setup => setup.AddFolder(Directory.GetCurrentDirectory()), tags: ["folder"]);
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("folder")
               });
           });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_healthy_if_no_folder_provided()
    {
        var webHostBuilder = new WebHostBuilder()
           .ConfigureServices(services =>
           {
               services.AddHealthChecks()
                   .AddFolder(setup =>
                   {
                   }, tags: ["folder"]);
           })
           .Configure(app =>
           {
               app.UseHealthChecks("/health", new HealthCheckOptions
               {
                   Predicate = r => r.Tags.Contains("folder")
               });
           });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task be_unhealthy_if_folder_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddFolder(setup => setup.AddFolder($"{Directory.GetCurrentDirectory()}/non-existing-folder"), tags: ["folder"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("folder")
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
