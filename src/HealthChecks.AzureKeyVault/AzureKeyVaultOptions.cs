using System;
using System.Collections.Generic;

namespace HealthChecks.AzureKeyVault
{
    /// <summary>
    /// Azure KeyVault configuration options.
    /// </summary>
    public class AzureKeyVaultOptions
    {
        internal HashSet<string> _secrets = new HashSet<string>();

        internal IEnumerable<string> Secrets
        {
            get { return _secrets; }
        }

        internal string KeyVaultUrlBase { get; private set; }
        internal string ClientId { get; private set; }
        internal string ClientSecret { get; private set; }
        internal bool UseManagedServiceIdentity { get; private set; } = true;
        internal AzureServiceTokenProviderOptions TokenProviderOptions = new AzureServiceTokenProviderOptions();


        /// <summary>
        /// Configures remote Azure Key Vault Url service
        /// </summary>
        /// <param name="keyVaultUrlBase">The azure KeyVault url base like  https://[vaultname].vault.azure.net/ .</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions UseKeyVaultUrl(string keyVaultUrlBase)
        {
            if (!Uri.TryCreate(keyVaultUrlBase, UriKind.Absolute, out var _))
            {
                throw new ArgumentException("KeyVaultUrlBase must be a valid Uri.");
            }

            KeyVaultUrlBase = keyVaultUrlBase;
            return this;
        }

        /// <summary>
        /// Azure key vault connection is performed using provided Client Id and Client Secret.
        /// </summary>
        /// <param name="clientId">Registered application Id</param>
        /// <param name="clientSecret">Registered application secret</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions UseClientSecrets(string clientId, string clientSecret)
        {
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException("ClientId and ClientSecret parameters should not be empty.");
            }

            ClientId = clientId;
            ClientSecret = clientSecret;
            UseManagedServiceIdentity = false;
            return this;
        }

        /// <summary>
        /// Azure key vault connection is performed using Azure Managed Service Identity.
        /// </summary>
        /// <param name="setup">Optional action to configure the Azure Service Token Provider.</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions UseAzureManagedServiceIdentity(Action<AzureServiceTokenProviderOptions> setup = null)
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            UseManagedServiceIdentity = true;            
            setup?.Invoke(TokenProviderOptions);
            return this;
        }

        /// <summary>
        /// Add a Azure Key Vault secret to be checked
        /// </summary>
        /// <param name="secretIdentifier"></param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddSecret(string secretIdentifier)
        {
            _secrets.Add(secretIdentifier);

            return this;
        }
    }
}