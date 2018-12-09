using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Azure.KeyVault.KeyVaultClient;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultMsiHealthCheck : IHealthCheck
    {
        private readonly AzureKeyVaultOptions _keyVaultOptions;

        public AzureKeyVaultMsiHealthCheck(AzureKeyVaultOptions keyVaultOptions)
        {
            if (string.IsNullOrEmpty(keyVaultOptions.KeyVaultUrlBase))
            {
                throw new ArgumentNullException(nameof(keyVaultOptions.KeyVaultUrlBase));
            }

            _keyVaultOptions = keyVaultOptions;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var currentSecret = string.Empty;

            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                var client = new KeyVaultClient(new AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

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
    }
}
