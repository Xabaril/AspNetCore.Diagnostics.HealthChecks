## Azure KeyVault Health Check

This health check verifies the ability to communicate with [Azure Key Vault Secrets](https://azure.microsoft.com/services/key-vault/). It uses the provided [SecretClient](https://learn.microsoft.com/dotnet/api/azure.security.keyvault.secrets.secretclient) to get configured secret. When the connection to the service itself can be made, but secret is not found, it returns `HealthStatus.Healthy` status.

### Defaults

By default, the `SecretClient` instance is resolved from service provider. `AzureKeyVaultSecretOptions` by default uses "AzureKeyVaultSecretsHealthCheck" secret name and does not try to create the secret when it's not found.

```csharp
public void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new SecretClient(new Uri("azure-key-vault-uri"), new DefaultAzureCredential()));
    builder.AddAzureKeyVaultSecrets();
}
```

### Customization

You can additionally add the following parameters:

- `secretClientFactory`: A factory method to provide `SecretClient` instance.
- `optionsFactory`: A factory method to provide `AzureKeyVaultSecretOptions` instance. It allows to specify the secret name and whether the secret should be created when it's not found.
- `healthCheckName`: The health check name. The default is `azure_key_vault_secret`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

```csharp
public void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp => new SecretClient(new Uri("azure-key-vault-uri"), new DefaultAzureCredential()));
    builder.AddAzureKeyVaultSecrets(
        optionsFactory: sp => new AzureKeyVaultSecretOptions()
        {
            SecretName = "demo"
        });
}
```

### Performance

 When the secret is not found, the secret client throws `RequestFailedException`. The health check catches it, but it's expensive in terms of performance.

That is why it's recommended to create the secret before using the health check. It can be done by using `AzureKeyVaultSecretOptions.CreateWhenNotFound`, but it requires secret set permissions. Such permissions should not be assigned just for the purpose of using this health check!

For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/dotnet/api/overview/azure/identity-readme)
