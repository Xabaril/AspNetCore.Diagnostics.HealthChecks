namespace HealthChecks.Azure.KeyVault.Keys;

public sealed class AzureKeyVaultKeysHealthCheckOptions
{
    private string _keyName = nameof(AzureKeyVaultKeysHealthCheck);

    public string KeyName
    {
        get => _keyName;
        set => _keyName = Guard.ThrowIfNull(value, throwOnEmptyString: true, paramName: nameof(KeyName));
    }
}
