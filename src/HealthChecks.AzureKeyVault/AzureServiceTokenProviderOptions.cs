using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HealthChecks.AzureKeyVault
{
    /// <summary>
    /// Azure KeyVault configuration options.
    /// </summary>
    public class AzureServiceTokenProviderOptions
    {
        internal string ConnectionString { get; private set; }
        internal string AzureAdInstance { get; private set; } = "https://login.microsoftonline.com/";

        /// <summary>
        /// Configures connection string for the Azure Service Token Provider
        /// </summary>
        /// <param name="connectionString">The connection string for the Azure Service Token Provider</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureServiceTokenProviderOptions UseConnectionString(string connectionString)
        {            
            // Validate connection string format
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                HashSet<string> connectionSettingsKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Split by ;
                string[] splitted = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string splitSetting in splitted)
                {
                    // Remove spaces before and after key=value
                    string setting = splitSetting.Trim();

                    // If setting is empty, continue. This is an empty space at the end e.g. "key=value; "
                    if (setting.Length == 0)
                        continue;

                    if (setting.Contains("="))
                    {
                        // Key is the first part before =
                        string[] keyValuePair = setting.Split('=');
                        string key = keyValuePair[0].Trim();

                        // Value is everything else as is
                        var value = setting.Substring(keyValuePair[0].Length + 1).Trim();

                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            if (!connectionSettingsKeys.Contains(key))
                            {
                                connectionSettingsKeys.Add(key);
                            }
                            else
                            {
                                throw new ArgumentException(
                                    $"Connection string {connectionString} is not in a proper format. Key '{key}' is repeated.");
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"Connection string {connectionString} is not in a proper format. Expected format is Key1=Value1;Key2=Value=2;");
                    }
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(connectionString));
            }    

            ConnectionString = connectionString;
            return this;
        }

        /// <summary>
        /// Configures the Azure AD instance for the Azure Service Token Provider
        /// </summary>
        /// <param name="azureAdInstance">The Azure AD instance for the Azure Service Token Provider</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureServiceTokenProviderOptions UseAzureAdInstance(string azureAdInstance) 
        {
            if (string.IsNullOrEmpty(azureAdInstance))
            {
                throw new ArgumentNullException(nameof(azureAdInstance));
            }

            if (!Uri.TryCreate(azureAdInstance, UriKind.Absolute, out var _))
            {
                throw new ArgumentException($"azureAdInstance {azureAdInstance} must be a valid Uri");
            }

            if (!azureAdInstance.ToLower().StartsWith("https"))
            {
                throw new ArgumentException($"azureAdInstance {azureAdInstance} is not valid. It must use https.");
            }

            AzureAdInstance = azureAdInstance;
            return this;
        }
    }
}