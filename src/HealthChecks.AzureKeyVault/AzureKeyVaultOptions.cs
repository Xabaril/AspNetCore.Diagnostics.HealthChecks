using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultOptions
    {
        internal List<string> Secrets { get; } =  new List<string>();
        /// <summary>
        /// Azure Key Vault base url - https://[vaultname].vault.azure.net/
        /// </summary>
        public string KeyVaultUrlBase { get; set; }
        /// <summary>
        /// Registered application Id
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Registered application secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Add a Azure Key Vault secret to be checked
        /// </summary>
        /// <param name="secretIdentifier"></param>
        /// <returns></returns>
        public AzureKeyVaultOptions AddSecret(string secretIdentifier)
        {
            if(!Secrets.Contains(secretIdentifier))
            {
                Secrets.Add(secretIdentifier);
            }

            return this;
        }
    }
}
