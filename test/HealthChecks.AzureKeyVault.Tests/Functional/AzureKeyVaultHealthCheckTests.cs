using System.Net;
using Azure.Identity;

namespace HealthChecks.AzureKeyVault.Tests.Functional;

public class AzureKeyVaultHealthCheckTests
{
    // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/785
    [Fact]
    public async Task be_unhealthy_if_keyvalue_is_not_available()
    {
        var webHostBuilder = WebApplication.CreateBuilder();
                    webHostBuilder.Services.AddHealthChecks()
                        .AddAzureKeyVault(
                            new Uri("https://www.thisisnotarealurl.com"),
                            new DefaultAzureCredential(),
                            setup =>
                            {
                                //setup
                                //    .UseKeyVaultUrl("https://www.thisisnotarealurl2.com")
                                //    .UseAzureManagedServiceIdentity();
                            });
        webHostBuilder.WebHost.UseTestServer();
        await using var app = webHostBuilder.Build();
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                    });
        await app.StartAsync();


        using var response = await app.GetTestClient().GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
