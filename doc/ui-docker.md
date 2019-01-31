# HealthChecks UI Docker Image

*HealthChecks* is available as a docker image on [DockerHub](https://hub.docker.com/r/xabarilcoding/healthchecksui/). This image is a simple ASP.NET Core project with the *HealthCheckUI* middleware.

```bash
docker pull xabarilcoding/healthchecksui
docker run --name ui -p 5000:80 -d xabarilcoding/healthchecksui:latest
```

You can use environment variables to configure all properties on *HealthChecksUI*. 

```bash
docker run --name ui -p 5000:80 -e 'HealthChecks-UI:HealthChecks:0:Name=httpBasic' -e 'HealthChecks-UI:HealthChecks:0:Uri=http://the-healthchecks-server-path' -d healthchecksui:latest
```

Read the [DockerHub full description](https://hub.docker.com/r/xabarilcoding/healthchecksui/) to get more information about HealthChecksUI docker configuration.
