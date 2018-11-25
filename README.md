
[![Build status](https://ci.appveyor.com/api/projects/status/ldk031dvcn2no51g?svg=true)](https://ci.appveyor.com/project/Xabaril/aspnetcore-diagnostics-healthchecks) 

[![Build history](https://buildstats.info/appveyor/chart/xabaril/aspnetcore-diagnostics-healthchecks)](https://ci.appveyor.com/project/xabaril/aspnetcore-diagnostics-healthchecks/history)


# AspNetCore.Diagnostics.HealthChecks

This project is a [BeatPulse](http://github.com/xabaril/beatpulse) liveness and UI *port* to new *Microsoft Health Checks* feature included on **ASP.NET Core 2.2**.

## Health Checks

HealthChecks packages include health checks for:

- Sql Server
- MySql
- Oracle
- Sqlite
- Postgres 
- EventStore
- RabbitMQ
- Redis 
- System: Disk Storage, Private Memory, Virtual Memory
- Azure Service Bus: EventHub, Queue and Topics
- Azure Storage: Blob, Queue and Table
- Azure DocumentDb
- Amazon DynamoDb
- Network: Ftp, SFtp, Dns, Tcp port, Smtp, Imap
- Mongo
- Kafka
- Identity Server
- Uri: single uri and uri groups

``` PowerShell
Install-Package AspNetCore.HealthChecks.System
Install-Package AspNetCore.HealthChecks.Network
Install-Package AspNetCore.HealthChecks.SqlServer
Install-Package AspNetCore.HealthChecks.MongoDb
Install-Package AspNetCore.HealthChecks.Npgsql
Install-Package AspNetCore.HealthChecks.Redis
Install-Package AspNetCore.HealthChecks.EventStore
Install-Package AspNetCore.HealthChecks.AzureStorage
Install-Package AspNetCore.HealthChecks.AzureServiceBus
Install-Package AspNetCore.HealthChecks.MySql
Install-Package AspNetCore.HealthChecks.DocumentDb
Install-Package AspNetCore.HealthChecks.SqLite
Install-Package AspNetCore.HealthChecks.Kafka
Install-Package AspNetCore.HealthChecks.RabbitMQ
Install-Package AspNetCore.HealthChecks.IdSvr
Install-Package AspNetCore.HealthChecks.DynamoDB
Install-Package AspNetCore.HealthChecks.Oracle
Install-Package AspNetCore.HealthChecks.Uris
```

Once the package is installed you can add the HealthCheck using the **AddXXX** extension methods.

> We use [MyGet](https://www.myget.org/F/xabaril/api/v3/index.json) feed for preview versions of HealthChceks pacakges.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddSqlServer(Configuration["Data:ConnectionStrings:Sql"])
        .AddRedis(Configuration["Data:ConnectionStrings:Redis"]);
}
```

Each HealthCheck registration supports also name, tags, failure status and other optional parameters.

```csharp
public void ConfigureServices(IServiceCollection services)
{
      services.AddHealthChecks()
          .AddSqlServer(
              connectionString: Configuration["Data:ConnectionStrings:Sql"],
              healthQuery: "SELECT 1;",
              name: "sql", 
              failureStatus: HealthStatus.Degraded,
              tags: new string[] { "db", "sql", "sqlserver" });
}
```

## HealthCheck push results

HealthChecks include a *push model* to send HealthCheckReport results into configured consumers. The project **HealthChecks.Publisher.ApplicationInsights** define a consumer to send report results to Application Insights.

Include the package in your project:

```powershell
install-package HealthChecks.Publisher.ApplicationInsights
```

Add publisher into the *IHealthCheckBuilder*

```csharp
services.AddHealthChecks()
        .AddSqlServer(connectionString: Configuration["Data:ConnectionStrings:Sample"])
        .AddCheck<RandomHealthCheck>("random")
        .AddApplicationInsightsPublisher();
```

## HealthCheckUI and failure notifications

The project HealthChecks.UI is a minimal UI interface that stores and shows the health checks results from the configured HealthChecks uris. 

To integrate HealthChecks.UI in your project you just need to add the HealthChecks.UI services and middlewares.

```csharp
public class Startup
{       
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecksUI();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app.UseHealthChecksUI();
    }
}
```

This automatically registers a new interface on **/health-ui**. 

> Optionally, *UseHealthChecksUI* can be configured with different UI response path.

![HealthChecksUI](./doc/images/ui-home.png)

**HealthChecksUI** is also available as a *docker image*  You can read more about [HealthChecks UI Docker image](./doc/ui-docker.md).

### Configuration

By default HealthChecks returns a simple Status Code (200 or 503) without the HealthReport data. If you want that HealthCheck-UI shows the HealthReport data from your HealthCheck you can enable it adding an specific ResponseWriter.

```csharp
 app.UseHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

> *WriteHealthCheckUIResponse* is defined on HealthChecks.UI.Client nuget package.

To show these HealthChecks in HealthCheck-UI they have to be configured through the **HealthCheck-UI** settings. 

```json
{
  "HealthCheck-UI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:6457/health"
      }
    ],
    "Webhooks": [
      {
        "Name": "",
        "Uri": "",
        "Payload": "",
        "RestoredPayload":""
      }
    ],
    "EvaluationTimeOnSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications":60
  }
}
```

    1.- HealthChecks: The collection of health checks uris to evaluate.
    2.- EvaluationTimeOnSeconds: Number of elapsed seconds between health checks.
    3.- Webhooks: If any health check returns a *Failure* result, this collections will be used to notify the error status. (Payload is the json payload and must be escaped. For more information see the notifications documentation section)
    4.- MinimumSecondsBetweenFailureNotifications: The minimum seconds between failure notifications to avoid receiver flooding.

All health checks results are stored into a SqLite database persisted to disk with *healthcheckdb* name.

### Failure Notifications

If the **WebHooks** section is configured, HealthCheck-UI automatically posts a new notification into the webhook collection. HealthCheckUI uses a simple replace method for values in the webhook's **Payload** and **RestorePayload** properties. At this moment we support two bookmarks:

[[LIVENESS]] The name of the liveness that returns *Down*.

[[FAILURE]] A detail message with the failure.

The [web hooks section](./doc/webhooks.md) contains more information and webhooks samples for Microsoft Teams, Azure Functions, Slack and more.

## Contributing

AspNetCore.Diagnostics.HealthChecks wouldn't be possible without the time and effort of its contributors. The team is made up of Unai Zorrilla Castro [@unaizorrilla](https://github.com/unaizorrilla), Luis Ruiz Pavón [@lurumad](https://github.com/lurumad), Carlos Landeras [@carloslanderas](https://github.com/carloslanderas) and Eduard Tomás [@eiximenis](https://github.com/eiximenis).

*Our valued committers are*: Hugo Biarge @hbiarge, Matt Channer @mattchanner, Luis Fraile @lfraile, Bradley Grainger @bgrainger, Simon Birrer @SbiCA, Mahamadou Camara @poumup.

If you want to contribute to the project and make it better, your help is very welcome. You can contribute with helpful bug reports, features requests and also submitting new features with pull requests.

1. Read and follow the [Don't push your pull requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/)
2. Build.ps1 is working on local and AppVeyor.
3. Follow the code guidelines and conventions.
4. New features are not only code, tests and documentation are also mandatory.

