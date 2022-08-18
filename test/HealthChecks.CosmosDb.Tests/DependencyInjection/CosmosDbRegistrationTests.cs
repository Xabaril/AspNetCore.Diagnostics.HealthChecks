using Azure.Core;
using Microsoft.Azure.Cosmos;
using NSubstitute;

namespace HealthChecks.CosmosDb.Tests.DependencyInjection
{
    public class cosmosdb_registration_should
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("cosmosdb", null, null)]
        [InlineData(null, "my-cosmosdb-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("cosmosdb", "my-azureblob-group", HealthStatus.Degraded)]
        public void add_health_check_when_properly_configured(string? databaseId, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddCosmosDb(
                    $"AccountEndpoint=https://unit-test.documents.azure.com:443/;AccountKey={Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 })}",
                    database: databaseId,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "cosmosdb");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<CosmosDbHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("cosmosdb", null, null)]
        [InlineData(null, "my-cosmosdb-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("cosmosdb", "my-azureblob-group", HealthStatus.Degraded)]
        public void add_health_check_with_endpoint_when_properly_configured(string? databaseId, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddCosmosDb(
                    "https://unit-test.documents.azure.com:443/",
                    Substitute.For<TokenCredential>(),
                    database: databaseId,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "cosmosdb");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<CosmosDbHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("cosmosdb", null, null)]
        [InlineData(null, "my-cosmosdb-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData(null, null, null, "first-collection", "second_collections")]
        [InlineData("cosmosdb", "my-azureblob-group", HealthStatus.Degraded, "first-collection", "second_collections")]
        public void add_health_check_with_client_from_service_provider(string? databaseId, string? registrationName, HealthStatus? failureStatus, params string[] containerIds)
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton(Substitute.For<CosmosClient>())
                .AddHealthChecks()
                .AddCosmosDb(
                    o =>
                    {
                        o.ContainerIds = containerIds;
                        o.DatabaseId = databaseId;
                    },
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "cosmosdb");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<CosmosDbHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("cosmosdb", null, null)]
        [InlineData(null, "my-cosmosdb-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData(null, null, null, "first-collection", "second_collections")]
        [InlineData("cosmosdb", "my-azureblob-group", HealthStatus.Degraded, "first-collection", "second_collections")]
        public void add_health_check_with_client_from_service_provider_and_advanced_delegate(string? databaseId, string? registrationName, HealthStatus? failureStatus, params string[] containerIds)
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton(Substitute.For<CosmosClient>())
                .AddHealthChecks()
                .AddCosmosDb(
                    (sp, o) =>
                    {
                        o.ContainerIds = containerIds;
                        o.DatabaseId = databaseId;
                    },
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "cosmosdb");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<CosmosDbHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("cosmosdb", null, null)]
        [InlineData(null, "my-cosmosdb-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData(null, null, null, "first-collection", "second_collections")]
        [InlineData("cosmosdb", "my-azureblob-group", HealthStatus.Degraded, "first-collection", "second_collections")]
        public void add_collection_health_check_when_properly_configured(string? databaseId, string? registrationName, HealthStatus? failureStatus, params string[] containerIds)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddCosmosDbCollection(
                    $"AccountEndpoint=https://unit-test.documents.azure.com:443/;AccountKey={Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 })}",
                    database: databaseId,
                    collections: containerIds,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "cosmosdb");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<CosmosDbHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("cosmosdb", null, null)]
        [InlineData(null, "my-cosmosdb-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData(null, null, null, "first-collection", "second_collections")]
        [InlineData("cosmosdb", "my-azureblob-group", HealthStatus.Degraded, "first-collection", "second_collections")]
        public void add_collection_health_check_with_endpoint_when_properly_configured(string? databaseId, string? registrationName, HealthStatus? failureStatus, params string[] containerIds)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddCosmosDbCollection(
                    "https://unit-test.blob.core.windows.net",
                    Substitute.For<TokenCredential>(),
                    database: databaseId,
                    collections: containerIds,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "cosmosdb");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<CosmosDbHealthCheck>();
        }
    }
}
