## Kubernetes liveness and readiness probes

Asp.Net Core HealthChecks becomes really useful to configure our liveness and readiness probes in our kubernetes deployments. In the following lines we will show a sample of how can we do this.

To follow the sample you will need to have docker and kubernetes running on your local computer, because we are going to build an image locally and we are not going to publish it. Remember that if you are using Windows and you are using minikube instead of docker for windows you have to set the docker environment to aim the minikube vm by executing **eval $(minikube docker-env)** in a bash window so docker sends the build context to the minikube machine and the k8s cluster can see the local images.

First of all, we can create a new Empty Asp.Net Core application called **WebApp* and add the **Microsoft.AspNetCore.Diagnostics.HealthChecks** package to add health the health checks capabilities to our application. Remember to set the Docker support checkbox to have the project Dockerfile automatically created.

Next, we are going to configure a health check to represent the health of the service itself represented under the name "self".

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

```

After this, we are going to install the health check packages for sql server and redis that will be the dependencies we use in our application. Open a terminal
located where the csproj was created:

>>dotnet add package AspNetCore.HealthChecks.SqlServer

>>dotnet add package Install-Package AspNetCore.HealthChecks.Redis

Once you confirm the packages are installed, we are going to configure this two health checks under a tag called "services"

```csharp
   services.AddHealthChecks()
       .AddCheck("self", () => HealthCheckResult.Healthy())
       .AddSqlServer("Server=server;Database=master;User Id=sa;Password=pass",
              tags: new[] {"services"})
       .AddRedis("redis",
              tags: new[] {"services"});
```

The registered health checks will return a 200 OK status code when they are healthy and a 503 (service unavailable) when they are unhealthy. We will benefit from this status codes when we create http based checks on kubernetes.

Next step, we are going to register the paths that will return our health reports:

First, we are going to register the path for the "self" check, where the own application will tell us about it's health. Right now, we are always returning a healthy state, but for now, it's ok.

Let's register our endpoint under "/self" path, telling the registrations predicate we wan't to return only the report of the registration called "self". Go to Configure method in the Startup.cs and paste the following code block:


```csharp

    app.UseHealthChecks("/self", new HealthCheckOptions
    {
       Predicate = r => r.Name.Contains("self")
    });
```

Launch the project and open a browser or a curl request to 
http://localhost:{port}/self. The ouput should be the following:

>>clanderas@Mephisto:~$ curl  http://localhost:5000/self
>>Healthy

Our application self check is on place, returning a 200 status code.

Now is the turn to configure the service dependencies under the path "/ready"
because once we are working with kubernetes we wan't this path to represent if our service is working and ready because all their dependencies are working as well:

```csharp
 app.UseHealthChecks("/ready", new HealthCheckOptions
 {
    Predicate = r => r.Tags.Contains("services")
 });
```

To finish this section, we are going to change a little bit our self health check to be able to switch the application health status from healthy to unhealthy and viceversa:

define a class variable called:

```csharp
bool running = true;
```

and register the path called /switch in the pipeline with the following code:

```csharp
app.Map("/switch", appBuilder =>
{
      appBuilder.Run(async context =>
    {
          running = !running;
          await context.Response.WriteAsync($"{Environment.MachineName} running {running}");
    });
 });
```

The previous method will give us the ability to switch the in-memory "running" variable and we will return the health report based on it's value:

Replace the self check we registered like this:

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

```

to this:

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => running ? HealthCheckResult.Healthy()
                                    : HealthCheckResult.Unhealthy());

```

We are set!, we have the /self path to represent our application status. Think about what kind of code will you need to express the health you are working on. Maybe a call to an MVC controller that shall be always responding?.

The next think we are going to do is use the Dockerfile that Visual studio 2017 has automatically created for us and lets build a docker image.

Open a terminal and locate your path where the project sln is located and execute the following command to build the docker image:

>> docker build -t webapp . -f WebApp/Dockerfile 




