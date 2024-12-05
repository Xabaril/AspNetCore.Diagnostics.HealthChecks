## Datadog Health Check

This health check verifies the ability to communicate with [Datadog](https://www.datadoghq.com/). It uses the provided [DogStatsdService](https://docs.datadoghq.com/developers/dogstatsd/?tab=hostagent&code-lang=dotnet#dogstatsd-client) to record a run status for the specified named service check.

### Defaults

By default, the `DogStatsdService` instance is resolved from service provider. You need to specify the name of the custom check that will be published to Datadog.

```csharp
void Configure(IHealthChecksBuilder builder)
{
    builder.Services.AddSingleton(sp =>
    {
        StatsdConfig config = new() { StatsdServerName = "127.0.0.1" };
        DogStatsdService service = new();
        service.Configure(config);
        return service;
    });
    builder.AddDatadogPublisher(serviceCheckName: "myservice.healthchecks");
}
```

### Customization

You can use the overload that requires a `StatsdConfig` factory method. In such case, the health check is going to create a dedicated instance of `DogStatsdService` that will be used only by the health check itself.


```csharp
void Customization(IHealthChecksBuilder builder)
{
    builder.AddDatadogPublisher(
        serviceCheckName: "myservice.healthchecks",
        sp => new StatsdConfig()
        {
            StatsdServerName = "127.0.0.1",
            StatsdPort = 123,
        });
}
```
