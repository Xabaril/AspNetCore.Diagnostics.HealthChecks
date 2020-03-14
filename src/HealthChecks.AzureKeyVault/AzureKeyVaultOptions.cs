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

        internal HashSet<string> _keys = new HashSet<string>();

        internal IEnumerable<string> Keys
        {
            get { return _keys; }
        }

        internal List<(string, bool)> _certificates = new List<(string key, bool checkExpired)>();

        internal List<(string, bool)> Certificates
        {
            get { return _certificates; }
        }

        internal string KeyVaultUrlBase { get; private set; }
        internal string ClientId { get; private set; }
        internal string ClientSecret { get; private set; }
        internal bool UseManagedServiceIdentity { get; private set; } = true;

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
        /// <param name="keyVaultUrlBase">Azure Key Vault base url - https://[vaultname].vault.azure.net/ </param>
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
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions UseAzureManagedServiceIdentity()
        {
            ClientId = string.Empty;
            ClientSecret = string.Empty;
            UseManagedServiceIdentity = true;
            return this;
        }

        /// <summary>
        /// Add a Azure Key Vault secret to be checked
        /// </summary>
        /// <param name="secretIdentifier">The secret to be checked</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddSecret(string secretIdentifier)
        {
            _secrets.Add(secretIdentifier);

            return this;
        }

        /// <summary>
        /// Add a Azure Key Vault cryptographic key to be checked
        /// </summary>
        /// <param name="keyIdentifier">The cryptographic key to be checked</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddKey(string keyIdentifier)
        {
            _keys.Add(keyIdentifier);

            return this;
        }

        /// <summary>
        /// Add a Azure Key Vault certificate key to be checked
        /// </summary>
        /// <param name="certificateIdentifier">The certificate key to be checked</param>
        /// /// <param name="checkExpired">Certificate expiration date should be checked. It the certificate is expired a exception will be thrown</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddCertificate(string certificateIdentifier, bool checkExpired = false)
        {
            _certificates.Add((certificateIdentifier, checkExpired));

            return this;
        }
    }
}