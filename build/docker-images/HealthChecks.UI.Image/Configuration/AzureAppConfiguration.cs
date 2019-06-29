using System;
using System.Security.Policy;

namespace HealthChecks.UI.Image.Configuration
{
    public class AzureAppConfiguration
    {
        public static bool Enabled => 
            HasValue(AzureAppConfigurationKeys.Enabled) ? true : false;

        public static bool UseConnectionString => 
            HasValue(AzureAppConfigurationKeys.ConnectionString) ? true : false;

        public static bool UseLabel => 
            HasValue(AzureAppConfigurationKeys.Label) ? true : false;

        public static string ConnectionString => 
            GetValue(AzureAppConfigurationKeys.ConnectionString);

        public static string ManagedIdentityEndpoint => 
            GetValue(AzureAppConfigurationKeys.ManagedIdentityEndpoint);

        public static string Label => 
            GetValue(AzureAppConfigurationKeys.Label);

        private static string GetValue(string variable) => 
            Environment.GetEnvironmentVariable(variable);
        
        private static bool HasValue(string variable) => 
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable));
    }
   
}