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

        /// <summary>
        /// Add a Azure Key Vault secret to be checked
        /// </summary>
        /// <param name="secretName">The secret to be checked</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddSecret(string secretName)
        {
            _secrets.Add(secretName);

            return this;
        }

        /// <summary>
        /// Add a Azure Key Vault cryptographic key to be checked
        /// </summary>
        /// <param name="keyName">The cryptographic key to be checked</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddKey(string keyName)
        {
            _keys.Add(keyName);

            return this;
        }

        /// <summary>
        /// Add a Azure Key Vault certificate key to be checked
        /// </summary>
        /// <param name="certificateName">The certificate key to be checked</param>
        /// /// <param name="checkExpired">Certificate expiration date should be checked. It the certificate is expired a exception will be thrown</param>
        /// <returns><see cref="AzureKeyVaultOptions"/></returns>
        public AzureKeyVaultOptions AddCertificate(string certificateName, bool checkExpired = false)
        {
            _certificates.Add((certificateName, checkExpired));

            return this;
        }
    }
}