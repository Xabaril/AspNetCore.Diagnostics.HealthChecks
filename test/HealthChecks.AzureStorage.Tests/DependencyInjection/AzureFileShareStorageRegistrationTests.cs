using Azure.Storage.Files.Shares;
using NSubstitute;

namespace HealthChecks.AzureStorage.Tests.DependencyInjection
{
    public class azurefilesharestorage_registration_should
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("share", null, null)]
        [InlineData(null, "my-azurefileshare-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("share", "my-azurefileshare-group", HealthStatus.Degraded)]
        public void add_health_check_when_properly_configured(string? shareName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddAzureFileShare(
                    "FileEndpoint=https://unit-test.file.core.windows.net",
                    shareName: shareName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azurefileshare");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<AzureFileShareHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("share", null, null)]
        [InlineData(null, "my-azurefileshare-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("share", "my-azurefileshare-group", HealthStatus.Degraded)]
        public void add_health_check_with_client_from_service_provider(string? shareName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton(Substitute.For<ShareServiceClient>())
                .AddHealthChecks()
                .AddAzureFileShare(
                    (sp, o) => o.ShareName = shareName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azurefileshare");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<AzureFileShareHealthCheck>();
        }
    }
}
