using System.Net;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace HealthChecks.AzureKeyVault.Tests.Functional;

public class AzureKeyVaultSecretsHealthCheckTests
{
    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullSecretClient()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new AzureKeyVaultSecretsHealthCheck(secretClient: null!, secretName: "notNull"));

        Assert.Equal("secretClient", argumentNullException.ParamName);
    }

    [Fact]
    public void CtorThrowsArgumentNullExceptionForNullSecretName()
    {
        ArgumentNullException argumentNullException = Assert.ThrowsAny<ArgumentNullException>(
            () => new AzureKeyVaultSecretsHealthCheck(
                secretClient: new SecretClient(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential()),
                secretName: null!));
        Assert.Equal("secretName", argumentNullException.ParamName);
    }

    [Fact]
    public void CtorThrowsArgumentExceptionForEmptySecretName()
    {
        ArgumentException argumentException = Assert.ThrowsAny<ArgumentException>(
            () => new AzureKeyVaultSecretsHealthCheck(
                secretClient: new SecretClient(new Uri("https://www.thisisnotarealurl.com"), new DefaultAzureCredential()),
                secretName: string.Empty));
        Assert.Equal("secretName", argumentException.ParamName);
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
                        instance: new AzureKeyVaultSecretsHealthCheck(secretClient),
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
