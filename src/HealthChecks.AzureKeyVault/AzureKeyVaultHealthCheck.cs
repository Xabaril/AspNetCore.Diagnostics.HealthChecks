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
        private readonly TimeSpan _timeout;

        public AzureKeyVaultHealthCheck(AzureKeyVaultOptions options, TimeSpan timeout)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var currentSecret = string.Empty;
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    using (var client = CreateClient())
                    {
                        foreach (var item in _options.Secrets)
                        {
                            await client.GetSecretAsync(_options.KeyVaultUrlBase, item, timeoutCancellationTokenSource.Token);
                        }

                        return HealthCheckResult.Healthy();
                    }
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
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
