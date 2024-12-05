# OpenID Connect Server Health Check

This health check verifies that an [OpenID Connect](https://openid.net/developers/how-connect-works/) server (like [Duende IdentityServer](https://duendesoftware.com/products/identityserver), [Microsoft Entra ID](https://www.microsoft.com/en-us/security/business/microsoft-entra), etc.) is responding. Internally, it downloads the discovery document and checks existence of document's properties.

## Example Usage

Use the following snippet to register an OpenID Connect server accessible at https://myoidcserver.com.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddOpenIdConnectServer(oidcSvrUri: new Uri("https://myoidcserver.com"));
}
```

Use the following snippet to additionally specify a different discovery endpoint path. The default value if not specified is `.well-known/openid-configuration`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddOpenIdConnectServer(
            oidcSvrUri: new Uri("https://myoidcserver.com"),
            discoverConfigurationSegment: "discovery/endpoint");
}
```

You can additionally specify the following parameters:

- `name`: The health check name. Default if not specified is `oidcserver`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
