# **Digital Twin Health Check**

Azure Digital Twins is an Internet of Things (IoT) platform that enables you to create a digital representation of real-world things, places, business processes, and people.

For more information about Azure Digital Twin please check [Azure Digital Twin Home](https://azure.microsoft.com/en-us/services/digital-twins/)

This health check can check the Digital Twin:

- liveness connection status.
- state of the model definition
- status of an instance

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name.
  <br/>Default for liveness if not specified is `azuredigitaltwin`.
  <br/>Default for model state if not specified is `azuredigitaltwinmodels`.
  <br/>Default for model state if not specified is `azuredigitaltwininstance`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

---

## _Digital Twin Liveness Health Check_

This health check provide the liveness status for the Azure Digital Twin resource client connection

### Example Usage

You can add health check with the default client arguments...

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureDigitalTwin(
            "MyDigitalTwinClientId",
            "MyDigitalTwinClientSecret",
            "TenantId")
}
```

... or with the service client credentials flow that you want:

```cs
public void ConfigureServices(IServiceCollection services)
{
    ServiceClientCredentials myCredentials = <my_credentials_flow>;
    services
        .AddHealthChecks()
        .AddAzureDigitalTwin(
            myCredentials)
}
```

---

## _Digital Twin Model Health Check_

This health check receives a list of models ids, and check if the Digital Twin has all models match with them.
If the health check detect an `out of sync` models return the data with those elements:

- `unregistered`: those models that exist in model definition but not in the Digital Twin

### Example Usage

<br/>_C# Configuration:_

You can also add health check with the default client arguments...

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinModels(
            "MyDigitalTwinClientId",
            "MyDigitalTwinClientSecret",
            "TenantId",
            "https://my-awesome-dt-host",
            new string[] { "my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1" })
}
```

... or with the token credentials flow that you want:

```cs
public void ConfigureServices(IServiceCollection services)
{
    TokenCredentials myCredentials = <my_credentials_flow>;
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinModels(
            myCredentials,
            new string[] { "my:dt:definition_a;1", "my:dt:definition_b;1", "my:dt:definition_c;1" },
            failureStatus: HealthStatus.Degraded)
}
```

<small>NOTE: This sample provides a Degraded status if this Health Check fails because it will check for a non sync model state (instead of a real connection status), and the resource is responding at the client call.</small>

<br/>_Failure status response:_

```json
azuredigitaltwinmodels:
{
  data:
  {
    unregistered: [ "my:dt:definition_b;1" ]
  },
  description: "The digital twin is out of sync with the models provided",
  duration: "00:00:17.6056085",
  exception: null,
  status: 1,
  tags: [ "ready" ]
}
```

---

## _Digital Twin Instance Health Check_

This health check returns the status of a given instance.

### Example Usage

<br/>_C# Configuration:_

You can also add health check with the default client arguments...

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinInstance(
            "MyDigitalTwinClientId",
            "MyDigitalTwinClientSecret",
            "TenantId",
            "https://my-awesome-dt-host",
            "my_dt_instance_name")
}
```

... or with the token credentials flow that you want:

```cs
public void ConfigureServices(IServiceCollection services)
{
    TokenCredentials myCredentials = <my_credentials_flow>;
    services
        .AddHealthChecks()
        .AddAzureDigitalTwinModels(
            myCredentials,
            "https://my-awesome-dt-host",
            "my_dt_instance_name")
}
```
