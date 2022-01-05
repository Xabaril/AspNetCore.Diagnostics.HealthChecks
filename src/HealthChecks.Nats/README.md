# NATS Health Check

This health check verifies the ability to communicate with a [NATS server](https://nats.io/about/). \
It relies on `NATS.Client` package version `0.14.3` at the time this was developed. \
Latest tag for the [official dockerhub image](https://hub.docker.com/_/nats/) is `2.6.6`.

## Builder Extension

```cs
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddNats((NatsOptions setup) => {
            setup.Url = "A string containing the URL (or URLs) to the NATS Server.";
            setup.CredentialsPath = "The full path to a chained credentials file.";
            setup.Jwt = "The path to a user's public JWT credentials.";
            setup.PrivateNKey = "The path to a file for user user's private Nkey seed.";
        });
}
```

`Url` property is mandatory. \
The rest of the properties in `NatsOptions` are optional. \
There is a demo instance `nats://demo.nats.io:4222` managed by nats.io and the docker image produces `nats://localhost:4222`.

The setup action used by the extension method caters for all three overloads supplied by the Nats client to create a connection off of a connection factory.

```cs
namespace NATS.Client
{
    public sealed class ConnectionFactory
    {
        public IConnection CreateConnection(string url);
        public IConnection CreateConnection(string url, string credentialsPath);
        public IConnection CreateConnection(string url, string jwt, string privateNkey);
    }
}
```

Like all `IHealthChecksBuilder` extensions, all the following parameters have type `default` values and may be overridden:

- `name`: The health check name. Default if not specified is `nats`.
- `failureStatus`: The `HealthStatus` that should be reported when the health check fails. Default is `HealthStatus.Unhealthy`.
- `tags`: A list of tags that can be used to filter sets of health checks.
- `timeout`: A `System.TimeSpan` representing the timeout of the check.

[<<](../../README.md)