using BeatPulse.AzureServiceBus;
using BeatPulse.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeatPulse
{
    public static class BeatPulseContextExtensions
    {
        public static BeatPulseContext AddAzureEventHub(this BeatPulseContext context, string connectionString, string eventHubName, string name = nameof(AzureEventHubLiveness), string defaultPath = "azureeventhub")
        {
            return context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp=>new AzureEventHubLiveness(connectionString, eventHubName,sp.GetService<ILogger<AzureEventHubLiveness>>()));
            });
        }

        public static BeatPulseContext AddAzureServiceBusQueue(this BeatPulseContext context, string connectionString, string queueName, string name = nameof(AzureServiceBusQueueLiveness), string defaultPath = "azureservicebusqueue")
        {
            return context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp => new AzureServiceBusQueueLiveness(connectionString, queueName, sp.GetService<ILogger<AzureServiceBusQueueLiveness>>()));
            });
        }

        public static BeatPulseContext AddAzureServiceBusTopic(this BeatPulseContext context, string connectionString, string topicName, string name = nameof(AzureServiceBusTopicLiveness), string defaultPath = "azureservicebustopic")
        {
            return context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp => new AzureServiceBusTopicLiveness(connectionString, topicName, sp.GetService<ILogger<AzureServiceBusTopicLiveness>>()));
            });
        }
    }
}
