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
            var currentSecret = string.Empty;
            try
            {
                using (var client = CreateClient())
                {
                    foreach (var item in _options.Secrets)
                    {
                        await client.GetSecretAsync(_options.KeyVaultUrlBase, item, cancellationToken);
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
