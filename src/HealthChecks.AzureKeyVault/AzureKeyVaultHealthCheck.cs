using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultHealthCheck : IHealthCheck
    {
        private readonly AzureKeyVaultOptions _options;
        private readonly Uri _keyVaultUri;
        private readonly TokenCredential _azureCredential;

        private static readonly ConcurrentDictionary<Uri, SecretClient> _secretClientsHolder = new ConcurrentDictionary<Uri, SecretClient>();
        private static readonly ConcurrentDictionary<Uri, KeyClient> _keyClientsHolder = new ConcurrentDictionary<Uri, KeyClient>();
        private static readonly ConcurrentDictionary<Uri, CertificateClient> _certificateClientsHolder = new ConcurrentDictionary<Uri, CertificateClient>();

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
                    await secretClient.GetSecretAsync(secret, cancellationToken: cancellationToken);
                }

                foreach (var key in _options.Keys)
                {
                    var keyClient = CreateKeyClient();
                    await keyClient.GetKeyAsync(key, cancellationToken: cancellationToken);
                }

                foreach (var (key, checkExpired) in _options.Certificates)
                {
                    var certificateClient = CreateCertificateClient();
                    var certificate = await certificateClient.GetCertificateAsync(key, cancellationToken: cancellationToken);

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
            if (!_keyClientsHolder.TryGetValue(_keyVaultUri, out KeyClient client))
            {
                client = new KeyClient(_keyVaultUri, _azureCredential);
                _keyClientsHolder.TryAdd(_keyVaultUri, client);
            }

            return client;
        }

        private SecretClient CreateSecretClient()
        {
            if (!_secretClientsHolder.TryGetValue(_keyVaultUri, out SecretClient client))
            {
                client = new SecretClient(_keyVaultUri, _azureCredential);
                _secretClientsHolder.TryAdd(_keyVaultUri, client);
            }

            return client;
        }

        private CertificateClient CreateCertificateClient()
        {
            if (!_certificateClientsHolder.TryGetValue(_keyVaultUri, out CertificateClient client))
            {
                client = new CertificateClient(_keyVaultUri, _azureCredential);
                _certificateClientsHolder.TryAdd(_keyVaultUri, client);
            }

            return client;
        }
    }
}
