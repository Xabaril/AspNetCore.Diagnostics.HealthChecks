## Azure IoT Hub Health Check

This health check verifies the ability to communicate with Azure IoT Hub. For more information about Azure IoT Hub please check and .NET please check the [Azure IoT Hub Microsoft Site](https://azure.microsoft.com/en-us/services/iot-hub/)

### Defaults
note: AddRegistryReadCheck() or AddRegistryWriteCheck() is required to be called else it will return unhealthy for the Registry manager health check

```csharp
public void ConfigureServices(IServiceCollection services)
{
    Services.AddSingleton(sp => RegistryManager.Create("iot-hub-hostname", new DefaultAzureCredential());
    Services.AddSingleton(sp => ServiceClient.Create("iot-hub-hostname", new DefaultAzureCredential());

    services
        .AddHealthChecks()
        .AddAzureIoTHubRegistryManager(
            clientFactory: sp.GetRequiredService<RegistryManager>()
            optionsFactory: sp => new IotHubRegistryManagerOptions()
              .AddRegistryReadCheck()
              .AddRegistryWriteCheck();
       .AddAzureIoTHubServiceClient();
}
```


### Customization

With all of the following examples, you can additionally add the following parameters:

AddAzureIoTHubServiceClient
- `serviceClientFactory`: A factory method to provide `ServiceClient` instance.
- `optionsFactory`: A factory method to provide `IotHubServiceClientOptions` instance. It allows to specify the secret name and whether the secret should be created when it's not found.
- `name`: The health check name. Default if not specified is `iothub`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

AddAzureIoTHubRegistryManager
- `registryManagerFactory`: A factory method to provide `RegistryManager` instance.
- `optionsFactory`: A factory method to provide `IotHubRegistryManagerOptions` instance. It allows to specify the secret name and whether the secret should be created when it's not found.
- `name`: The health check name. Default if not specified is `iothub`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
