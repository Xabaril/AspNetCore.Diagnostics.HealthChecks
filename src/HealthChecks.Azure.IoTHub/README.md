## Azure IoT Hub Health Check

This health check verifies the ability to communicate with Azure IoT Hub. For more information about Azure IoT Hub please check and .NET please check the [Azure IoT Hub Microsoft Site](https://azure.microsoft.com/services/iot-hub/)

### Defaults

You can use `RegistryManager` or `ServiceClient` or both. It's recommended to have a single instance per application, so prefer the type you already use.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddSingleton(sp => ServiceClient.Create("iot-hub-hostname", new DefaultAzureCredential()))
        .AddHealthChecks()
        .AddAzureIoTHubServiceClient();

    // or

    services
        .AddSingleton(sp => RegistryManager.Create("iot-hub-hostname", new DefaultAzureCredential()))
        .AddHealthChecks()
        .AddAzureIoTHubRegistryReadCheck();
}
```


### Customization

With all of the following examples, you can additionally add the following parameters:

AddAzureIoTHubServiceClient
- `serviceClientFactory`: An optional factory method to provide `ServiceClient` instance.
- `name`: The health check name.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

AddAzureIoTHubRegistryManager
- `registryManagerFactory`: An optional factory method to provide `RegistryManager` instance.
- `query`: A query to perform by the read health check.
- `deviceId`: The id of the device to add and remove.
- `name`: The health check name.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.
