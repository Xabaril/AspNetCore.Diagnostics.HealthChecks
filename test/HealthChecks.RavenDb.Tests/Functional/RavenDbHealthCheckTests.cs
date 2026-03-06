using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HealthChecks.RavenDB;
using HealthChecks.UI.Client;

namespace HealthChecks.RavenDb.Tests.Functional;

public class ravendb_healthcheck_should(RavenDbContainerFixture ravenDbFixture) : IClassFixture<RavenDbContainerFixture>
{
    private readonly string[] _urls = [ravenDbFixture.GetConnectionString()];

    [Fact]
    public async Task be_healthy_if_ravendb_is_available()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ => _.Urls = _urls, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_healthy_if_ravendb_is_available_and_contains_specific_database()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "Demo";
                    }, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_timeout_is_too_low()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "Demo";
                        _.RequestTimeout = TimeSpan.FromMilliseconds(0.001);
                    }, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_not_available()
    {
        var connectionString = "http://localhost:9999";

        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ => _.Urls = [connectionString], tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task be_unhealthy_if_ravendb_is_available_but_database_doesnot_exist()
    {
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddHealthChecks()
                    .AddRavenDB(_ =>
                    {
                        _.Urls = _urls;
                        _.Database = "ThisDatabaseReallyDoesnExist";
                    }, tags: ["ravendb"]);
            })
            .Configure(app =>
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = r => r.Tags.Contains("ravendb"),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
            });

        using var server = new TestServer(webHostBuilder);

        using var response = await server.CreateRequest("/health").GetAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task not_dispose_shared_certificate_when_store_initialization_fails()
    {
        using var rsa = RSA.Create(2048);
        var certificateRequest = new CertificateRequest("CN=ravendb-healthcheck-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

        var options = new RavenDBOptions
        {
            Urls = ["http://localhost:0"],
            Certificate = certificate
        };

        var healthCheck = new RavenDBHealthCheck(options);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("ravendb", _ => healthCheck, HealthStatus.Unhealthy, tags: null)
        };

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.ShouldBe(HealthStatus.Unhealthy);

        using var privateKey = certificate.GetRSAPrivateKey();
        privateKey.ShouldNotBeNull();
    }
}
