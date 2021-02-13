# Azure IoT Hub Health Check

This health check verifies the ability to communicate with Azure IoT Hub. For more information about Azure IoT Hub please check and .NET please check the [Azure IoT Hub Microsoft Site](https://azure.microsoft.com/en-us/services/iot-hub/)

## Example Usage

With all of the following examples, you can additionally add the following parameters:

- `name`: The health check name. Default if not specified is `iothub`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

### Basic

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddAzureIoTHub(options=>
        {
            options.AddConnectionString("iot-hub-connectionstring")
                .AddServiceConnectionCheck();
        });
}
```