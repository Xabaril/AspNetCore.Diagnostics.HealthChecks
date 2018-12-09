using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultOptions
    {
        internal List<string> Secrets { get; } =  new List<string>();      
        internal string KeyVaultUrlBase { get; set; }       
        internal string ClientId { get; set; }       
        internal string ClientSecret { get; set; }


        /// <summary>
        /// Configures remote Azure Key Vault Url service
        /// </summary>
        /// <param name="keyVaultUrlBase"></param>
        /// <returns></returns>
        public AzureKeyVaultOptions UseKeyVaultUrl(string keyVaultUrlBase)
        {
            KeyVaultUrlBase = keyVaultUrlBase;
            return this;
        }

        /// <summary>
        /// Azure key vault connection is performed using provided Client Id and Client Secret
        /// </summary>
        /// <param name="keyVaultUrlBase">Azure Key Vault base url - https://[vaultname].vault.azure.net/ </param>
        /// <param name="clientId">Registered application Id</param>
        /// <param name="clientSecret">Registered application secret</param>
        /// <returns></returns>
        public AzureKeyVaultOptions UseClientSecrets(string clientId, string clientSecret)
        {            
            if(string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentNullException("ClientId and ClientSecret parameters should not be empty");
            }
            
            ClientId = clientId;
            ClientSecret = clientSecret;

            return this;
        }

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
