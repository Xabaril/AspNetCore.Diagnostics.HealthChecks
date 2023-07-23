using System.Net;
using Azure.Identity;

namespace HealthChecks.AzureKeyVault.Tests.Functional;

public class AzureKeyVaultHealthCheckTests
{
    // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/785
    [Fact]
    public async Task be_unhealthy_if_keyvalue_is_not_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .AddAzureKeyVault(
                        new Uri("https://www.thisisnotarealurl.com"),
                        new DefaultAzureCredential(),
                        setup =>
                        {
                            //setup
                            //    .UseKeyVaultUrl("https://www.thisisnotarealurl2.com")
                            //    .UseAzureManagedServiceIdentity();
                        });
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                });
            });

        using var server = new TestServer(webHostBuilder);

        var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
