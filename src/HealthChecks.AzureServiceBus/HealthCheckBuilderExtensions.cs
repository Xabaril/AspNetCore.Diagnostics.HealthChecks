using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string AZUREEVENTHUB_NAME = "azureeventhub";
        const string AZUREQUEUE_NAME = "azurequeue";
        const string AZURETOPIC_NAME = "azuretopic";

        public static IHealthChecksBuilder AddAzureEventHub(this IHealthChecksBuilder builder, string connectionString, string eventHubName)
        {
            return builder.Add(new HealthCheckRegistration(
                AZUREEVENTHUB_NAME,
                sp => new AzureEventHubHealthCheck(connectionString, eventHubName, sp.GetService<ILogger<AzureEventHubHealthCheck>>()),
                null,
                new string[] { AZUREEVENTHUB_NAME }));
        }

        public static IHealthChecksBuilder AddAzureServiceBusQueue(this IHealthChecksBuilder builder, string connectionString, string queueName)
        {
            return builder.Add(new HealthCheckRegistration(
                AZUREQUEUE_NAME,
                sp => new AzureServiceBusQueueHealthCheck(connectionString, queueName, sp.GetService<ILogger<AzureServiceBusQueueHealthCheck>>()),
                null,
                new string[] { AZUREQUEUE_NAME }));
        }

        public static IHealthChecksBuilder AddAzureServiceBusTopic(this IHealthChecksBuilder builder, string connectionString, string topicName)
        {
            return builder.Add(new HealthCheckRegistration(
                AZURETOPIC_NAME,
                sp => new AzureServiceBusTopicHealthCheck(connectionString, topicName, sp.GetService<ILogger<AzureServiceBusTopicHealthCheck>>()),
                null,
                new string[] { AZURETOPIC_NAME }));
        }
    }
}
