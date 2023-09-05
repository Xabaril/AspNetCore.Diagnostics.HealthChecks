## Azure KeyVault Health Check

This health check verifies the ability to communicate with Azure Key Vault and the existence of some secrets, keys and certificates. For more information about Azure Key Vault check the [Azure KeyVault Microsoft Site](https://azure.microsoft.com/en-us/services/key-vault/)

### Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `azurekeyvault`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureKeyVault(new Uri("azure-key-vault-uri"), new DefaultAzureCredential(), options =>
        {
            options
                .AddSecret("secretname")
                .AddKey("keyname")
                .AddCertificate("certificatename");
        });
}
```

## Azure Key Vault Secrets Health Check

This health check verifies the ability to communicate with Azure Key Vault Secrets. It uses the provided `SecretClient` to get given secret (a default name "AzureKeyVaultSecretsHealthCheck" is provided, so the users don't have to configure anything). When the secret is not found, it will try to create it if `createWhenNotFound` was set to `true`. When the secret is not found, but `createWhenNotFound` is `false`, it returns `HealthStatus.Healthy` status,
    as the connection to the service itself can be made. `createWhenNotFound` is false by default. Enabling it requires secret set permissions and can be used to improve performance, as secret not found is signaled via `RequestFailedException`.

### Sample

```csharp
private void ConfigureServices(IServiceCollection services)
    => services.AddHealthChecks()
        .Add(new HealthCheckRegistration(
            "azure_key_vault_secrets",
            serviceProvider => new AzureKeyVaultSecretsHealthCheck(serviceProvider.GetRequiredService<SecretClient>()),
            failureStatus: HealthStatus.Unhealthy,
            tags: default,
            timeout: default));
```


For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)
