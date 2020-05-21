using System;
using HealthChecks.UI.Image.Configuration.Helpers;

namespace HealthChecks.UI.Image.Configuration
{
    public class AzureAppConfiguration
    {
        public static bool Enabled
        {
            get
            {
                if (EnvironmentVariable.HasValue(AzureAppConfigurationKeys.Enabled))
                {
                    if (Boolean.TryParse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.Enabled)
                        , out bool enabled))
                    {
                        return enabled;
                    }

                    return false;
                }

                return false;
            }
        }

        public static bool UseConnectionString => 
            EnvironmentVariable.HasValue(AzureAppConfigurationKeys.ConnectionString) ? true : false;

        public static bool UseLabel => 
            EnvironmentVariable.HasValue(AzureAppConfigurationKeys.Label) ? true : false;

        public static string ConnectionString => 
            EnvironmentVariable.GetValue(AzureAppConfigurationKeys.ConnectionString);

        public static string ManagedIdentityEndpoint => 
            EnvironmentVariable.GetValue(AzureAppConfigurationKeys.ManagedIdentityEndpoint);

        public static string Label => 
            EnvironmentVariable.GetValue(AzureAppConfigurationKeys.Label);
    }
}