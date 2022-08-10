using Azure.Core;
using Azure.Storage.Queues;
using NSubstitute;

namespace HealthChecks.AzureStorage.Tests.DependencyInjection
{
    public class azurequeuestorage_registration_should
    {
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("queue", null, null)]
        [InlineData(null, "my-azurequeue-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("queue", "my-azurequeue-group", HealthStatus.Degraded)]
        public void add_health_check_when_properly_configured(string? queueName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddAzureQueueStorage(
                    "QueueEndpoint=https://unit-test.queue.core.windows.net",
                    queueName: queueName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azurequeue");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<AzureQueueStorageHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("queue", null, null)]
        [InlineData(null, "my-azurequeue-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("queue", "my-azurequeue-group", HealthStatus.Degraded)]
        public void add_health_check_with_uri_when_properly_configured(string? queueName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddHealthChecks()
                .AddAzureQueueStorage(
                    new Uri("https://unit-test.queue.core.windows.net"),
                    Substitute.For<TokenCredential>(),
                    queueName: queueName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azurequeue");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<AzureQueueStorageHealthCheck>();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("queue", null, null)]
        [InlineData(null, "my-azurequeue-group", null)]
        [InlineData(null, null, HealthStatus.Degraded)]
        [InlineData("queue", "my-azurequeue-group", HealthStatus.Degraded)]
        public void add_health_check_with_client_from_service_provider(string? queueName, string? registrationName, HealthStatus? failureStatus)
        {
            using var serviceProvider = new ServiceCollection()
                .AddSingleton(Substitute.For<QueueServiceClient>())
                .AddHealthChecks()
                .AddAzureQueueStorage(
                    (sp, o) => o.QueueName = queueName,
                    name: registrationName,
                    failureStatus: failureStatus)
                .Services
                .BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.ShouldBe(registrationName ?? "azurequeue");
            registration.FailureStatus.ShouldBe(failureStatus ?? HealthStatus.Unhealthy);
            check.ShouldBeOfType<AzureQueueStorageHealthCheck>();
        }
    }
}
