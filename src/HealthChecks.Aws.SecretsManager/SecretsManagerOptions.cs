using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.SecretsManager;

public class SecretsManagerOptions
{
    public AWSCredentials? Credentials { get; set; }

    public RegionEndpoint? RegionEndpoint { get; set; }

    internal HashSet<string> Secrets { get; } = new HashSet<string>();

    /// <summary>
    /// Add an AWS Secrets Manager secret to be checked.
    /// </summary>
    /// <param name="secretName">The secret to be checked.</param>
    /// <returns>Reference to the same <see cref="SecretsManagerOptions"/> to allow further configuration.</returns>
    public SecretsManagerOptions AddSecret(string secretName)
    {
        Secrets.Add(secretName);

        return this;
    }
}
