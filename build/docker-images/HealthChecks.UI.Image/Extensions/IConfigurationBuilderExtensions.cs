using Azure.Identity;
using HealthChecks.UI.Image.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System;

namespace HealthChecks.UI.Image.Extensions
{
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder UseAzureAppConfiguration(this IConfigurationBuilder builder)
        {
            Action<AzureAppConfigurationOptions> setupConfig = AzureAppConfiguration.UseConnectionString switch
            {
                true => options => options.Connect(AzureAppConfiguration.ConnectionString),
                false => options => options.Connect(new Uri(AzureAppConfiguration.ManagedIdentityEndpoint), new ManagedIdentityCredential())
            };

            builder.AddAzureAppConfiguration(options =>
            {
                setupConfig(options);

                if (AzureAppConfiguration.UseCacheExpiration)
                {
                    options.ConfigureRefresh(config =>
                    {
                        config.SetCacheExpiration(TimeSpan.FromSeconds(AzureAppConfiguration.CacheExpiration));
                    });
                }

                if (AzureAppConfiguration.UseLabel)
                {
                    options.Select(KeyFilter.Any, AzureAppConfiguration.Label);
                };

            });

            return builder;
        }
    }
}
