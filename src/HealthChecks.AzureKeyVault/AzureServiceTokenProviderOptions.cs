using System;
using System.Collections.Generic;

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
        /// Configures remote Azure Key Vault Url service
        /// </summary>
        /// <param name="keyVaultUrlBase">The azure KeyVault url base like  https://[vaultname].vault.azure.net/ .</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureServiceTokenProviderOptions UseConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

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