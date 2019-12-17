using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.AzureKeyVault
{
    public class AzureKeyVaultHealthCheck : IHealthCheck
    {
        private readonly AzureKeyVaultOptions _options;
        public AzureKeyVaultHealthCheck(AzureKeyVaultOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var client = CreateClient())
                {
                    foreach (var secret in _options.Secrets)
                    {
                        await client.GetSecretAsync(_options.KeyVaultUrlBase, secret, cancellationToken);
                    }

                    foreach (var key in _options.Keys)
                    {
                        await client.GetKeyAsync(_options.KeyVaultUrlBase, key, cancellationToken);
                    }

                    foreach (var (key, checkExpired) in _options.Certificates)
                    {
                        var certificate = await client.GetCertificateAsync(_options.KeyVaultUrlBase, key, cancellationToken);

                        if (checkExpired && certificate.Attributes.Expires.HasValue)
                        {
                            var expirationDate = certificate.Attributes.Expires.Value;

                            if (expirationDate < DateTime.UtcNow)
                            {
                                throw new Exception($"The certificate with key {key} has expired with date {expirationDate}");
                            }
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
        private KeyVaultClient CreateClient()
        {
            if (_options.UseManagedServiceIdentity)
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            }
            else
            {
                return new KeyVaultClient(GetToken);
            }
        }
        private async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCred = new ClientCredential(_options.ClientId, _options.ClientSecret);
            var result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
            {
                throw new InvalidOperationException($"[{nameof(AzureKeyVaultHealthCheck)}] - Failed to obtain the JWT token");
            }
            return result.AccessToken;
        }
    }
}
