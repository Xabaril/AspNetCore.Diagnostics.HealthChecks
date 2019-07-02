# HealthChecks UI Docker Image

*HealthChecks* is available as a docker image on [DockerHub](https://hub.docker.com/r/xabarilcoding/healthchecksui/). This image is a simple ASP.NET Core project with the *HealthCheckUI* middleware.

```bash
docker pull xabarilcoding/healthchecksui
docker run --name ui -p 5000:80 -d xabarilcoding/healthchecksui:latest
```

You can use environment variables to configure all properties on *HealthChecksUI*. 

```bash
docker run --name ui -p 5000:80 -e 'HealthChecksUI:HealthChecks:0:Name=httpBasic' -e 'HealthChecksUI:HealthChecks:0:Uri=http://the-healthchecks-server-path' -d xabarilcoding/healthchecksui:latest
```

## Azure App Configuration Service

Since version 2.2.32, our docker image supports configuration by using Azure App Configuration service.

Configuring a large amount of healthchecks and webhooks will require to pass a lot of environment variables or mounting a volume so Asp.Net Core configuration providers can find a settings file containing the HealthChecks config section.

By using Azure App Configuration service you can centralize HealthChecks configuration in Azure and bind it directly to the executing container at ease.

You should use environment variables to configure Azure App Confuration Service (AAC from now for brevity) in the UI docker image.

The existing environment variables are explained below:

| Environment Variable  | Description   | Notes   |
|---|---|---|
| AAC_Enabled   | Enables AAC config provider   | Not set by default |
| AAC_ConnectionString   | Connection string to configuration service  | If set, Managed Service Identity won't be used   |
| AAC_ManagedIdentityEndpoint | Your AAC endpoint to connect using Managed Identity | Sample: https://your-endpoint.azconfig.io
| AAC_Label   | Filter configuration keys containing this label   | Sample: HealthChecksConfig  |


As table explains, if **AAC_ConnectionString** is set, the image will connect to AAC using that connection string.
If you want to connect using managed identity service only specify **AAC_ManagedIdentityEndpoint** environment variable.

## Samples

- Creating an Azure Container instance using a connection string:

```bash

az container create --resource-group group-name --name container-name -e 'AAC_Enabled=true' 'AAC_Label=HealthChecksConfig' 'AAC_ConnectionString=Endpoint={your_connectionstring}' --image xabarilcoding/healthchecksui:latest --dns-name-label dns-checks --ports 80

```

- Creating an Azure Container instance using managed service identity:

```bash

az container create --resource-group group-name  --name container-name -e 'AAC_Enabled=true' 'AAC_Label=HealthChecksConfig' 'AAC_ManagedIdentityEndpoint=https://your-endpoint.azconfig.io' --image xabarilcoding/healthchecksui:latest  --dns-name-label dns-checks-msi --ports 80 --assign-identity

```


Read the [DockerHub full description](https://hub.docker.com/r/xabarilcoding/healthchecksui/) to get more information about HealthChecksUI docker configuration.
