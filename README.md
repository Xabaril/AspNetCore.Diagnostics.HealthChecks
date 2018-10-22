
# AspNetCore.Diagnostics.HealthChecks

This project is a [BeatPulse](http://github.com/xabaril/beatpulse) liveness and UI *port* to new *Microsoft Health Checks* feature included on **ASP.NET Core 2.2**.

## Health Checks

HealthChecks packages include health checks for Sql Server, MySql, Oracle, Sqlite, Postgres, RabbitMQ, Redis, System: Disk Storage, Private Memory, Virtual Memory, Azure Service Bus: EventHub, Queue and Topics, Azure Storage: Blob, Queue and Table, Azure DocumentDb, Amazon DynamoDb, Network: Ftp, SFtp, Dns, Tcp port, Smtp, Impa, Mongo, Kafka, Identity Server and Uri: single uri and uri groups

``` PowerShell
Install-Package HealthChecks.System
Install-Package HealthChecks.Network
Install-Package HealthChecks.SqlServer
Install-Package HealthChecks.MongoDb
Install-Package HealthChecks.Npgsql
Install-Package HealthChecks.Redis
Install-Package HealthChecks.AzureStorage
Install-Package HealthChecks.AzureServiceBus
Install-Package HealthChecks.MySql
Install-Package HealthChecks.DocumentDb
Install-Package HealthChecks.SqLite
Install-Package HealthChecks.Kafka
Install-Package HealthChecks.RabbitMQ
Install-Package HealthChecks.IdSvr
Install-Package HealthChecks.DynamoDB
Install-Package HealthChecks.Oracle
Install-Package HealthChecks.Uris
```

Once the package is installed you can add the HealthCheck using the **AddXXX** extension methods.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddSqlServer(Configuration["Data:ConnectionStrings:Sample"])
        .AddRedis(Configuration["Data:ConnectionStrings:Redis"]);
}
```

Each HealthCheck registration support also name, tags, failure status and other optional parameters.

```csharp
 public void ConfigureServices(IServiceCollection services)
  {
      services.AddHealthChecks()
          .AddSqlServer(
              connectionString: Configuration["Data:ConnectionStrings:Sql"],
              healthQuery:"SELECT 1;",
              name: "sql", 
              failureStatus: HealthStatus.Degraded,
              tags: new string[] { "db", "sql", "sqlserver" });
  }
```

## HealthCheckUI and failure notifications

The project HealthChecks.UI is a minimal UI interface that stores and shows the health checks results from the configured HealthChecks uri's. 

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

By default HealthChecks return a simple Status Code (200 or 503 ) without the HealthReport data. If you want that HealthCheck-UI shows the HealthReport data from your HealthCheck you can  enable it adding an specific ResponseWriter.

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

    1.- HealthChecks: The collection of health checks uris to watch.
    2.- EvaluationTimeOnSeconds: Number of elapsed seconds between health checks.
    3.- Webhooks: If any health check return a *Failure* result, this collections will be used to notify the error status. (Payload is the json payload and must be scape. For mor information see Notifications section)
    4.- MinimumSecondsBetweenFailureNotifications: The minimun seconds between failure notifications in order not flooding the notification receiver.

All health checks results are stored into a SqLite database persisted to disk with *healthcheckdb* name.

### Failure Notifications

If the **WebHooks** section is configured, HealthCheck-UI automatically posts a new notification into the webhook collection. HealthCheckUI uses a simple replace method for values in the webhook's **Payload** and **RestorePayload** properties. At this moment we support two bookmarks:

[[LIVENESS]] The name of the liveness that returns *Down*.

[[FAILURE]] A detail message with the failure.

The [web hooks section](./doc/webhooks.md) contains more information and webhooks samples for Microsoft Teams, Azure Functions, Slack and more.

## Contributing

AspNetCore.Diagnostics.HealthChecks  wouldn't be possible without the time and effort of its contributors. The team is made up of Unai Zorrilla Castro @unaizorrilla, Luis Ruiz Pavón @lurumad, Carlos Landeras @carloslanderas and Eduard Tomás @eiximenis.

*Our valued committers are*: Hugo Biarge @hbiarge, Matt Channer @mattchanner, Luis Fraile @lfraile, Bradley Grainger @bgrainger.

If you want to contribute to a project and make it better, your help is very welcome. You can contribute with helpful bug reports, feature request and also new features with pull requests.

1. Read and follow the [Don't push your pull requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/)
2. Build.ps1 is working on local and AppVeyor.
3. Follow the code guidelines and conventions.
4. New features are not only code, tests and documentation are also mandatory.

