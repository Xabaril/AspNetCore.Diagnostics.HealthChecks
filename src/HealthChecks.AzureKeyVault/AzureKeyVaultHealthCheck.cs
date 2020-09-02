using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultHealthCheck : IHealthCheck
    {
        private readonly AzureKeyVaultOptions _options;
        private readonly Uri _keyVaultUri;
        private readonly TokenCredential _azureCredential;

        public AzureKeyVaultHealthCheck(Uri keyVaultUri, TokenCredential credential, AzureKeyVaultOptions options)
        {
            _keyVaultUri = keyVaultUri ?? throw new ArgumentNullException(nameof(keyVaultUri));
            _azureCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var secret in _options.Secrets)
                {
                    var secretClient = CreateSecretClient();
                    await secretClient.GetSecretAsync(_options.KeyVaultUrlBase, secret, cancellationToken);
                }

                foreach (var key in _options.Keys)
                {
                    var keyClient = CreateKeyClient();
                    await keyClient.GetKeyAsync(_options.KeyVaultUrlBase, key, cancellationToken);
                }

                foreach (var (key, checkExpired) in _options.Certificates)
                {
                    var certificateClient = CreateCertificateClient();
                    var certificate = await certificateClient.GetCertificateAsync(key, cancellationToken);

                    if (checkExpired && certificate.Value.Properties.ExpiresOn.HasValue)
                    {
                        var expirationDate = certificate.Value.Properties.ExpiresOn.Value;

                        if (expirationDate < DateTime.UtcNow)
                        {
                            throw new Exception($"The certificate with key {key} has expired with date {expirationDate}");
                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private KeyClient CreateKeyClient()
        {
            return new KeyClient(_keyVaultUri, _azureCredential);
        }

        private SecretClient CreateSecretClient()
        {
            return new SecretClient(_keyVaultUri, _azureCredential);
        }

        private CertificateClient CreateCertificateClient()
        {
            return new CertificateClient(_keyVaultUri, _azureCredential);
        }
    }
}
