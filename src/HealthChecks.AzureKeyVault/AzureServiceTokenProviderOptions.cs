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
            if (string.IsNullOrEmpty(connectionString))
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