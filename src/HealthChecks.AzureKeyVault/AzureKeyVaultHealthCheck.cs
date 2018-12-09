using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultHealthCheck : IHealthCheck
    {
        private readonly AzureKeyVaultOptions _keyVaultOptions;

        public AzureKeyVaultHealthCheck(AzureKeyVaultOptions keyVaultOptions)
        {
            if (!Uri.TryCreate(keyVaultOptions.KeyVaultUrlBase, UriKind.Absolute, out var _))
            {
                throw new ArgumentException("KeyVaultUrlBase must be a valid Uri");
            }

            _keyVaultOptions = keyVaultOptions;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var currentSecret = string.Empty;

            try
            {
                var client = GetClient(_keyVaultOptions);
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

        private KeyVaultClient GetClient(AzureKeyVaultOptions options)
        {
            if (string.IsNullOrEmpty(options.ClientId))
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                return new KeyVaultClient(new AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            }
            else
            {
                return new KeyVaultClient(GetToken);
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
