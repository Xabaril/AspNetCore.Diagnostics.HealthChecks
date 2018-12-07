using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultHealthCheck : IHealthCheck
    {
        private readonly AzureKeyVaultOptions _keyVaultOptions;

        public AzureKeyVaultHealthCheck(AzureKeyVaultOptions keyVaultOptions)
        {
            if (string.IsNullOrEmpty(keyVaultOptions.KeyVaultUrlBase)) throw new ArgumentNullException(keyVaultOptions.KeyVaultUrlBase);
            if (string.IsNullOrEmpty(keyVaultOptions.ClientId)) throw new ArgumentNullException(keyVaultOptions.ClientId);
            if (string.IsNullOrEmpty(keyVaultOptions.ClientSecret)) throw new ArgumentNullException(keyVaultOptions.ClientSecret);
                
            _keyVaultOptions = keyVaultOptions;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string currentSecret = string.Empty;

            try
            {
                var client = new KeyVaultClient(GetToken);
                foreach (var secretIdentifier in _keyVaultOptions.Secrets)
                {
                    currentSecret = secretIdentifier;
                    await client.GetSecretAsync(_keyVaultOptions.KeyVaultUrlBase, secretIdentifier, cancellationToken);
                }

                return HealthCheckResult.Healthy();

            }
            catch (Exception ex)
            {
                var secretException = new Exception($"{currentSecret} secret error - {ex.Message}", ex);
                return new HealthCheckResult(context.Registration.FailureStatus, exception: secretException);
            }
        }

        public async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(_keyVaultOptions.ClientId, _keyVaultOptions.ClientSecret);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException($"[{nameof(AzureKeyVaultHealthCheck)}] - Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}
