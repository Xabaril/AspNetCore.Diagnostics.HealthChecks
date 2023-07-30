using HealthChecks.UI.Image.Configuration.Helpers;

namespace HealthChecks.UI.Image.Configuration;

public class AzureAppConfiguration
{
    public static bool Enabled
    {
        get
        {
            return EnvironmentVariable.HasValue(AzureAppConfigurationKeys.Enabled) &&
                bool.TryParse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.Enabled), out bool enabled) &&
                enabled;
        }
    }

    public static bool UseConnectionString =>
        EnvironmentVariable.HasValue(AzureAppConfigurationKeys.ConnectionString);

    public static bool UseLabel =>
        EnvironmentVariable.HasValue(AzureAppConfigurationKeys.Label);

    public static bool UseCacheExpiration =>
        EnvironmentVariable.HasValue(AzureAppConfigurationKeys.CacheExpiration)
        && double.TryParse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.CacheExpiration), out var _);

    public static double CacheExpiration => double.Parse(EnvironmentVariable.GetValue(AzureAppConfigurationKeys.CacheExpiration)!);

    public static string? ConnectionString =>
        EnvironmentVariable.GetValue(AzureAppConfigurationKeys.ConnectionString);

    public static string? ManagedIdentityEndpoint =>
        EnvironmentVariable.GetValue(AzureAppConfigurationKeys.ManagedIdentityEndpoint);

    public static string? Label =>
        EnvironmentVariable.GetValue(AzureAppConfigurationKeys.Label);
}
