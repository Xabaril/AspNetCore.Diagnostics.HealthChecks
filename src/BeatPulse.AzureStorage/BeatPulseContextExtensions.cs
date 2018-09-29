using BeatPulse.AzureStorage;
using BeatPulse.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BeatPulse
{
    public static class BeatPulseContextExtensions
    {
        public static BeatPulseContext AddAzureBlobStorage(this BeatPulseContext context, string connectionString, string name = nameof(AzureBlobStorageLiveness), string defaultPath = "azureblobstorage")
        {
            return context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp => new AzureBlobStorageLiveness(connectionString, sp.GetService<ILogger<AzureBlobStorageLiveness>>()));
            });
        }

        public static BeatPulseContext AddAzureTableStorage(this BeatPulseContext context, string connectionString, string name = nameof(AzureTableStorageLiveness), string defaultPath = "azuretablestorage")
        {
            return context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp => new AzureTableStorageLiveness(connectionString, sp.GetService<ILogger<AzureTableStorageLiveness>>()));

            });
        }

        public static BeatPulseContext AddAzureQueueStorage(this BeatPulseContext context, string connectionString, string name = nameof(AzureQueueStorageLiveness), string defaultPath = "azurequeuestorage")
        {
            return context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp => new AzureQueueStorageLiveness(connectionString, sp.GetService<ILogger<AzureQueueStorageLiveness>>()));
            });
        }
    }
}
