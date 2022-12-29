using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.SecretsManager;

public class SecretsManagerHealthCheck : IHealthCheck
{
    private readonly SecretsManagerOptions _secretsManagerOptions;

    public SecretsManagerHealthCheck(SecretsManagerOptions secretsManagerOptions)
    {
        _secretsManagerOptions = Guard.ThrowIfNull(secretsManagerOptions);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSecretsManagerClient();
            foreach (var secret in _secretsManagerOptions.Secrets)
            {
                await CheckSecretAsync(client, secret, cancellationToken).ConfigureAwait(false);
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
        var credentialsProvided = _secretsManagerOptions.Credentials is not null;
        var regionProvided = _secretsManagerOptions.RegionEndpoint is not null;
        return (credentialsProvided, regionProvided) switch
        {
            (false, false) => new AmazonSecretsManagerClient(),
            (false, true) => new AmazonSecretsManagerClient(_secretsManagerOptions.RegionEndpoint),
            (true, false) => new AmazonSecretsManagerClient(_secretsManagerOptions.Credentials),
            (true, true) => new AmazonSecretsManagerClient(_secretsManagerOptions.Credentials, _secretsManagerOptions.RegionEndpoint)
        };
    }

    private async Task CheckSecretAsync(IAmazonSecretsManager client, string secretName, CancellationToken cancellationToken)
    {
        var request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
        };

        // Check the existence of the secret. If it does not throw it is a valid one (binary or not)
        _ = await client.GetSecretValueAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
