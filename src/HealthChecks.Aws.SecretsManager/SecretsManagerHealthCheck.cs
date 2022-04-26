using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.SecretsManager
{
    public class SecretsManagerHealthCheck : IHealthCheck
    {
        private readonly SecretsManagerOptions _secretsManagerOptions;

        public SecretsManagerHealthCheck(SecretsManagerOptions secretsManagerOptions)
        {
            _secretsManagerOptions = secretsManagerOptions ?? throw new ArgumentNullException(nameof(secretsManagerOptions));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = CreateSecretsManagerClient();
                foreach (var secret in _secretsManagerOptions.Secrets)
                {
                    _ = await GetSecretAsync(client, secret, cancellationToken);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private IAmazonSecretsManager CreateSecretsManagerClient()
        {
            IAmazonSecretsManager client;

            var credentialsProvided = _secretsManagerOptions.Credentials is not null;

            var regionProvided = _secretsManagerOptions.RegionEndpoint is not null;

            if (!credentialsProvided && !regionProvided)
            {
                client = new AmazonSecretsManagerClient();
            }
            else if (!credentialsProvided && regionProvided)
            {
                client = new AmazonSecretsManagerClient(_secretsManagerOptions.RegionEndpoint);
            }
            else if (credentialsProvided && regionProvided)
            {
                client = new AmazonSecretsManagerClient(_secretsManagerOptions.Credentials,
                    _secretsManagerOptions.RegionEndpoint);
            }
            else
            {
                client = new AmazonSecretsManagerClient(_secretsManagerOptions.Credentials);
            }

            return client;
        }

        private async Task<string> GetSecretAsync(IAmazonSecretsManager client, string secretName, CancellationToken cancellationToken)
        {
            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
            };

            var response = await client.GetSecretValueAsync(request, cancellationToken);

            // Decrypts secret using the associated KMS key.
            // Depending on whether the secret is a string or binary, one of these fields will be populated.
            if (response.SecretString == null)
            {
                var memoryStream = response.SecretBinary;
                var reader = new StreamReader(memoryStream);
                return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(await reader.ReadToEndAsync()));
            }

            return response.SecretString;
        }
    }
}
