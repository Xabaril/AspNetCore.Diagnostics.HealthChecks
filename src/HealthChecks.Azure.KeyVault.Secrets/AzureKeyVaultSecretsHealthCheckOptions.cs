using Azure;

namespace HealthChecks.Azure.KeyVault.Secrets;

/// <summary>
/// Settings for <see cref="AzureKeyVaultSecretsHealthCheck"/>.
/// </summary>
public sealed class AzureKeyVaultSecretsHealthCheckOptions
{
    private string _secretName = nameof(AzureKeyVaultSecretsHealthCheck);

    /// <summary>
    /// The name of the secret that will be fetched from Azure Key Vault.
    /// The default value is "AzureKeyVaultSecretsHealthCheck".
    /// </summary>
    public string SecretName
    {
        get => _secretName;
        set => _secretName = Guard.ThrowIfNull(value, throwOnEmptyString: true, paramName: nameof(SecretName));
    }

    /// <summary>
    /// A boolean value that indicates whether the secret should be created when it's not found.
    /// False by default.
    /// </summary>
    /// <remarks>
    /// Enabling it requires secret set permissions and can be used to improve performance
    /// (secret not found is signaled via <see cref="RequestFailedException"/>).
    /// </remarks>
    public bool CreateWhenNotFound { get; set; }
}
