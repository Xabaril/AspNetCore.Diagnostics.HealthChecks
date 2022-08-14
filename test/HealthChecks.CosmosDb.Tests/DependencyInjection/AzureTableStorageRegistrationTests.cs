using Azure.Core;
using Azure.Data.Tables;
using HealthChecks.CosmosDb;
using NSubstitute;

namespace HealthChecks.AzureStorage.Tests.DependencyInjection
{
    public class azuretablestorage_registration_should
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("tableName", null, null)]
        [InlineData(null, "my-azuretable-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("tableName", "my-azuretable-group", HealthStatus.Degraded)]
        public void add_health_check_when_properly_configured(string? tableName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddAzureTable(
                    "TableEndpoint=https://unit-test.table.core.windows.net",
                    tableName: tableName!,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azuretable");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<TableServiceHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("tableName", null, null)]
        [InlineData(null, "my-azuretable-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("tableName", "my-azuretable-group", HealthStatus.Degraded)]
        public void add_health_check_with_uri_when_properly_configured(string? tableName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddAzureTable(
                    new Uri("https://unit-test.table.core.windows.net"),
                    Substitute.For<TokenCredential>(),
                    tableName: tableName!,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azuretable");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<TableServiceHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("tableName", null, null)]
        [InlineData(null, "my-azuretable-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("tableName", "my-azuretable-group", HealthStatus.Degraded)]
        public void add_health_check_with_uri_and_shared_key_credential_when_properly_configured(string? tableName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddAzureTable(
                    new Uri("https://unit-test.table.core.windows.net"),
                    Substitute.For<TableSharedKeyCredential>(),
                    tableName: tableName!,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azuretable");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<TableServiceHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("tableName", null, null)]
        [InlineData(null, "my-azuretable-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("tableName", "my-azuretable-group", HealthStatus.Degraded)]
        public void add_health_check_with_client_from_service_provider(string? tableName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton(Substitute.For<TableServiceClient>())
                .AddHealthChecks()
                .AddAzureTable(
                    o => o.TableName = tableName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azuretable");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<TableServiceHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("tableName", null, null)]
        [InlineData(null, "my-azuretable-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("tableName", "my-azuretable-group", HealthStatus.Degraded)]
        public void add_health_check_with_client_from_service_provider_and_advanced_delegate(string? tableName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton(Substitute.For<TableServiceClient>())
                .AddHealthChecks()
                .AddAzureTable(
                    (sp, o) => o.TableName = tableName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azuretable");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<TableServiceHealthCheck>();
        }
    }
}
