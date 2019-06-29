using System;
using HealthChecks.UI.Image.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace HealthChecks.UI.Image.Extensions
{
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder UseAzureAppConfiguration(this IConfigurationBuilder builder)
        {
            if (AzureAppConfiguration.UseConnectionString)
            {
                builder.AddAzureAppConfiguration(AzureConfigurationSetup.WithConnectionString);
            }
            else
            {
                builder.AddAzureAppConfiguration(AzureConfigurationSetup.WithManagedIdentity);
            }

            return builder;
        }
    }

    internal class AzureConfigurationSetup
    {
        public static void WithConnectionString(AzureAppConfigurationOptions options)
        {
            options.Connect(AzureAppConfiguration.ConnectionString);
            
            if (AzureAppConfiguration.UseLabel)
            {
                options.Use(KeyFilter.Any, AzureAppConfiguration.Label);
            }
        }

        public static void WithManagedIdentity(AzureAppConfigurationOptions options)
        {
            options.ConnectWithManagedIdentity(AzureAppConfiguration.ManagedIdentityEndpoint);
            if (AzureAppConfiguration.UseLabel)
            {
                options.Use(KeyFilter.Any, AzureAppConfiguration.Label);
            }
        }
    }
}