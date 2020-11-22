# Azure KeyVault Health Check

This health check verifies the ability to communicate with Azure Key Vault and the existence of some secrets, keys and certificates. For more information about Azure Key Vault check the [Azure KeyVault Microsoft Site](https://azure.microsoft.com/en-us/services/key-vault/)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `azurekeyvault`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic


```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureKeyVault(new Uri("azure-key-vault-uri",new DefaultAzureCredential(),options=>
        {
            options.AddSecret("secretname")
                .AddKey("keyname")
                .AddCertificate("certificatename");
        });
}
```

For more information about credentials types please see [Azure TokenCredentials](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)