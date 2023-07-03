using Azure.Core;
using Azure.Storage.Blobs;
using NSubstitute;

namespace HealthChecks.AzureStorage.Tests.DependencyInjection;

public class azureblobstorage_registration_should
{
    [Theory]
    [InlineData(null, null, null)]
    [InlineData("container", null, null)]
    [InlineData(null, "my-azureblob-group", null)]
    [InlineData(null, null, HealthStatus.Degraded)]
    [InlineData("container", "my-azureblob-group", HealthStatus.Degraded)]
    public void add_health_check_when_properly_configured(string? containerName, string? registrationName, HealthStatus? failureStatus)
    {
        using var serviceProvider = new ServiceCollection()
            .AddHealthChecks()
            .AddAzureBlobStorage(
                "BlobEndpoint=https://unit-test.blob.core.windows.net",
                containerName: containerName,
                name: registrationName,
                failureStatus: failureStatus)
            .Services
            .BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(registrationName ?? "azureblob");
        registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
        check.ShouldBeOfType<AzureBlobStorageHealthCheck>();
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("container", null, null)]
    [InlineData(null, "my-azureblob-group", null)]
    [InlineData(null, null, HealthStatus.Degraded)]
    [InlineData("container", "my-azureblob-group", HealthStatus.Degraded)]
    public void add_health_check_with_uri_when_properly_configured(string? containerName, string? registrationName, HealthStatus? failureStatus)
    {
        using var serviceProvider = new ServiceCollection()
            .AddHealthChecks()
            .AddAzureBlobStorage(
                new Uri("https://unit-test.blob.core.windows.net"),
                Substitute.For<TokenCredential>(),
                containerName: containerName,
                name: registrationName,
                failureStatus: failureStatus)
            .Services
            .BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(registrationName ?? "azureblob");
        registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
        check.ShouldBeOfType<AzureBlobStorageHealthCheck>();
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("container", null, null)]
    [InlineData(null, "my-azureblob-group", null)]
    [InlineData(null, null, HealthStatus.Degraded)]
    [InlineData("container", "my-azureblob-group", HealthStatus.Degraded)]
    public void add_health_check_with_client_from_service_provider(string? containerName, string? registrationName, HealthStatus? failureStatus)
    {
        using var serviceProvider = new ServiceCollection()
            .AddSingleton(Substitute.For<BlobServiceClient>())
            .AddHealthChecks()
            .AddAzureBlobStorage(
                o => o.ContainerName = containerName,
                name: registrationName,
                failureStatus: failureStatus)
            .Services
            .BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(registrationName ?? "azureblob");
        registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
        check.ShouldBeOfType<AzureBlobStorageHealthCheck>();
    }
    [Theory]
    [InlineData(null, null, null)]
    [InlineData("container", null, null)]
    [InlineData(null, "my-azureblob-group", null)]
    [InlineData(null, null, HealthStatus.Degraded)]
    [InlineData("container", "my-azureblob-group", HealthStatus.Degraded)]
    public void add_health_check_with_client_from_delegate(string? containerName, string? registrationName, HealthStatus? failureStatus)
    {
        using var serviceProvider = new ServiceCollection()
            .AddHealthChecks()
            .AddAzureBlobStorage(
                clientFactory: sp => Substitute.For<BlobServiceClient>(),
                o => o.ContainerName = containerName,
                name: registrationName,
                failureStatus: failureStatus)
            .Services
            .BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(registrationName ?? "azureblob");
        registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
        check.ShouldBeOfType<AzureBlobStorageHealthCheck>();
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("container", null, null)]
    [InlineData(null, "my-azureblob-group", null)]
    [InlineData(null, null, HealthStatus.Degraded)]
    [InlineData("container", "my-azureblob-group", HealthStatus.Degraded)]
    public void add_health_check_with_client_from_service_provider_and_advanced_delegate(string? containerName, string? registrationName, HealthStatus? failureStatus)
    {
        using var serviceProvider = new ServiceCollection()
            .AddSingleton(Substitute.For<BlobServiceClient>())
            .AddHealthChecks()
            .AddAzureBlobStorage(
                (sp, o) => o.ContainerName = containerName,
                name: registrationName,
                failureStatus: failureStatus)
            .Services
            .BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(registrationName ?? "azureblob");
        registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
        check.ShouldBeOfType<AzureBlobStorageHealthCheck>();
    }
    [Theory]
    [InlineData(null, null, null)]
    [InlineData("container", null, null)]
    [InlineData(null, "my-azureblob-group", null)]
    [InlineData(null, null, HealthStatus.Degraded)]
    [InlineData("container", "my-azureblob-group", HealthStatus.Degraded)]
    public void add_health_check_with_client_from_delegate_and_advanced_delegate(string? containerName, string? registrationName, HealthStatus? failureStatus)
    {
        using var serviceProvider = new ServiceCollection()
            .AddHealthChecks()
            .AddAzureBlobStorage(
                clientFactory: sp => Substitute.For<BlobServiceClient>(),
                (sp, o) => o.ContainerName = containerName,
                name: registrationName,
                failureStatus: failureStatus)
            .Services
            .BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe(registrationName ?? "azureblob");
        registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
        check.ShouldBeOfType<AzureBlobStorageHealthCheck>();
    }
}
