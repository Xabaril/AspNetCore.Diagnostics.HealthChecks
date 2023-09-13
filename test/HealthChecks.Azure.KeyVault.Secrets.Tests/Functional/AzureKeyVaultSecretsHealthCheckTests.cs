using System.Net;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HealthChecks.Azure.KeyVault.Secrets;

namespace HealthChecks.AzureKeyVault.Tests.Functional;

public class AzureKeyVaultSecretsHealthCheckTests
{
    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullSecretClient()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new AzureKeyVaultSecretsHealthCheck(secretClient: null!, options: new AzureKeyVaultSecretOptions()));

        Assert.Equal("secretClient", argumentNullException.ParamName);
    }

    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullOptions()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new AzureKeyVaultSecretsHealthCheck(
                secretClient: new SecretClient(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential()),
                options: null!));

        Assert.Equal("options", argumentNullException.ParamName);
    }

    [Theory]
    [InlineData(HealthStatus.Unhealthy)]
    [InlineData(HealthStatus.Degraded)]
    public async Task ReturnsProvidedFailureStatusWhenConnectionCanNotBeMade(HealthStatus failureStatus)
    {
        SecretClientOptions clientOptions = new();
        clientOptions.Retry.MaxRetries = 0; // don't enable retries (test runs few times faster)
        SecretClient secretClient = new(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential(), clientOptions);

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddHealthChecks()
                    .Add(new HealthCheckRegistration(
                        name: "azure_key_vault_secrets",
                        instance: new AzureKeyVaultSecretsHealthCheck(secretClient, new AzureKeyVaultSecretOptions()),
                        failureStatus: failureStatus,
                        tags: null));
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync().ConfigureAwait(false);

        response.StatusCode.ShouldBe(failureStatus == HealthStatus.Unhealthy ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
    }
}
