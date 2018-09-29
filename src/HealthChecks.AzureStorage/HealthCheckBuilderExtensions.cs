using HealthChecks.AzureStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string AZURESTORAGE_NAME = "azureblob";
        const string AZURETABLE_NAME = "azuretable";
        const string AZUREQUEUE_NAME = "azurequeue";

        public static IHealthChecksBuilder AddAzureBlobStorage(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.Add(new HealthCheckRegistration(
               AZURESTORAGE_NAME,
               sp => new AzureBlobStorageHealthCheck(connectionString, sp.GetService<ILogger<AzureBlobStorageHealthCheck>>()),
               null,
               new string[] { AZURESTORAGE_NAME }));
        }

        public static IHealthChecksBuilder AddAzureTableStorage(this IHealthChecksBuilder builder, string connectionString, string name = nameof(AzureTableStorageHealthCheck), string defaultPath = "azuretablestorage")
        {
            return builder.Add(new HealthCheckRegistration(
               AZURETABLE_NAME,
               sp => new AzureTableStorageHealthCheck(connectionString, sp.GetService<ILogger<AzureTableStorageHealthCheck>>()),
               null,
               new string[] { AZURETABLE_NAME }));
        }

        public static IHealthChecksBuilder AddAzureQueueStorage(this IHealthChecksBuilder builder, string connectionString)
        {
            return builder.Add(new HealthCheckRegistration(
               AZUREQUEUE_NAME,
               sp => new AzureQueueStorageHealthCheck(connectionString, sp.GetService<ILogger<AzureQueueStorageHealthCheck>>()),
               null,
               new string[] { AZUREQUEUE_NAME }));
        }
    }
}
