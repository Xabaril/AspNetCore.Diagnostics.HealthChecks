using HealthChecks.UI.Image.Configuration;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {

        public static IServiceCollection AddStorageProvider(this HealthChecksUIBuilder builder, IConfiguration configuration)
        {
            string configuredStorage = configuration[UIKeys.STORAGE_PROVIDER];
            string connectionString = configuration[UIKeys.STORAGE_CONNECTION];

            if (string.IsNullOrEmpty(configuredStorage)) configuredStorage = StorageProviderEnum.InMemory.ToString();

            if (Enum.TryParse(configuredStorage, out StorageProviderEnum storageEnum))
            {
                var providers = Storage.GetProviders();

                var targetProvider = providers[storageEnum];

                if (targetProvider.RequiresConnectionString && string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException($"{UIKeys.STORAGE_CONNECTION} value has not been configured and it's required for {storageEnum.ToString()}");
                }

                Console.WriteLine($"Configuring image to work with {storageEnum} provider");

                targetProvider.SetupProvider(builder, connectionString);
            }
            else
            { 
                throw new ArgumentException($"Variable {UIKeys.STORAGE_PROVIDER} has an invalid value: {configuredStorage}." +
                    $" Available providers are {string.Join(" , ", Enum.GetNames(typeof(StorageProviderEnum)))}");
            }

            return builder.Services;
        }
    }
}
