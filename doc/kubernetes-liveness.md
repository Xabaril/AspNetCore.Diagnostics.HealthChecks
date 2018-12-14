## Kubernetes liveness and readiness probes using HealthChecks

Asp.Net Core HealthChecks becomes really useful to configure our liveness and readiness probes in our kubernetes deployments. In the following lines we will show a sample of how can we do this. This sample is a little tutorial that might be useful for people starting to work with HealthChecks and Kubernetes.

To follow the sample you will need to have docker and kubernetes running on your local computer, because we are going to build an image locally and we are not going to publish it. Remember that if you are using Windows and you are using minikube instead of docker for windows you have to set the docker environment to aim the minikube vm by executing **eval $(minikube docker-env)** in a bash window so docker sends the build context to the minikube machine and the k8s cluster can see the local images.

First of all, we can create a new Empty Asp.Net Core application called **WebApp** and add the **Microsoft.AspNetCore.Diagnostics.HealthChecks** package to add the health checks capabilities to our application. Remember to set the Docker support checkbox to have the project **Dockerfile** automatically created.

Next, we are going to configure a health check to represent the health of the service itself represented under the name "self".

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

```

After this, we are going to install the health check packages for sql server and redis that will be the dependencies we use in our application. Open a terminal
located where the csproj was created:

`dotnet add package AspNetCore.HealthChecks.SqlServer`

`dotnet add package AspNetCore.HealthChecks.Redis`

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

We are set!, we have the /self path to represent our application status. Think about what kind of code will you need to express the health of the app you are working on right now. Maybe a call to an MVC controller that shall be always responding?.

The next think we are going to do is using the Dockerfile that Visual studio 2017 has automatically created for us and build a docker image.

Open a terminal and locate your path where the project sln is located and execute the following command to build the docker image:

`docker build -t webapp . -f WebApp/Dockerfile`

Once our docker image is created, we are going to deploy to kubernetes 3 deployments. 

- One for sqlserver
- One for redis 

(both of them with a service exposing the containers to the cluster using ClusterIp)

- The third one will be a deployment that creates 3 replicas of our webapp thas is configured with Asp.Net Core Healthchecks (webapp image). This deployment will be exposed using a Nodeport bound to our host 30000 port. This Nodeport will do local load balancing among all our replicas.

To avoid pasting all the deployment and services yaml files within the tutorial I have created a gist on the following [link](https://gist.github.com/CarlosLanderas/4f651fdaa2342db55f66d41f405630c6).

Copy and paste the code and create both files locally: **infra.yml** and **deployment.yml**

Note: Take a look to the deployment.yml file and the **imagePullPolicy: Never**. That line is in charge of not trying to connect to docker hub and look for the image locally.


Once you have both files in place, be sure your kubernetes local cluster is running
and execute the following commands:

`kubectl apply -f infra.yml`

`kubectl apply -f deployment.yml`

The output of the commands should be:

```
deployment.extensions/sqlserver created

service/sqlserver created

deployment.extensions/redis created

service/redis created

deployment.extensions/sqlserver created

service/sqlserver created

deployment.extensions/redis created

service/redis created
```

While your pods are being created, let's take a look to our deployment.yml file and put special attention in the line 16:

```yaml
 livenessProbe:
          httpGet:
            path: /self
            port: 80
            scheme: HTTP
          initialDelaySeconds: 10
          periodSeconds: 15
```

As you can see, we are declaring the liveness probe that should be executed against each of our replicas in webapp deployment. This declaration will trigger an http request to the /self path we declared for the app health, giving and initial delay of **10 seconds of margin** to allow the pod initializing correcly. If we don't give **enough delay** so the pod can start the pod will be killed by the orchestrator all the time because we are not giving it time to be ready so we will find ourselves in a start-killed-loop

In the line 23 of the deployment yaml we can find the readiness probe described like this:

```yaml
    readinessProbe:
          httpGet:
            path: /ready
            port: 80
            scheme: HTTP
          initialDelaySeconds: 10
          periodSeconds: 15
```

As you can see the declaration is very similar, but this time we are targetting the path /ready where we are returning the health of all our application dependencies (sql and redis)


**IMPORTANT**: The difference between a liveness and a readiness check is the liveness check will tell k8s if our pod is healthy. 

If the pod is not healthy, k8s will kill the pod and restart it so it can get recovered.

The readiness check will tell k8s if our app is ready (In this case configured to tell if the dependencies are up and ready but it could apply to other scenarios like the application initialization that might be a long running process).

If some of the dependencies are broken or not ready, when the readiness check is triggered, k8s will notice that the pod is not ready and no traffic will be redirected to that pod. That means if we have 3 replicas and one of them is not ready, instead of sharing all the requests among the 3 replicas only 2 will be serving requests and the failing one will be out of traffic until it gets recovered.


>>Lets finish!

Our pods should be ready by now, lets check it by running:

`kubectl get pods`

We should see all the pods in a ready state (1/1) and Running

```
NAME                             READY   STATUS    RESTARTS   AGE
redis-5f6c496656-tbqdl           1/1     Running   0          14m
sqlserver-6645fb796-4tfwd        1/1     Running   0          14m
webapp-deploy-7686b8c794-fvfq9   1/1     Running   0          14m
webapp-deploy-7686b8c794-kzjrx   1/1     Running   0          14m
webapp-deploy-7686b8c794-w2nlk   1/1     Running   0          14m
```

To end this tutorial we are going to break some of the replicas by calling the switch mechanism we exposed on the /switch path. Have in mind, we are using a Nodeport
and the output from this will be different in your execution because it's random.

If you are running docker for windows, the webapp is listening in localhost:30000
and if you are using minikube you should run the following command to get the final url.

`minikube service webapp-service --url`

(The output should be something like this: http://192.168.99.100:30000
)

On the console, execute the following command:

`kubectl get pods --watch`

This will show the pods with watch mode, and we will see live updates whenever a pod status changes.

Open a new console and execute the following command to switch to unhealthy status some of the replicas:

`curl http://localhost:30000/switch`

As we are getting balanced, we can reach the same replica twice, so be sure to receive at least two messages in which the pod enters in a unhealthy state:

`webapp-deploy-7686b8c794-kzjrx running False`

`webapp-deploy-7686b8c794-kzjrx running False`

If we wait some seconds for the liveness probe to pass, we will see on the watch console that our pods have been killed by kubernetes showing a 1 number on the restarts column:

`webapp-deploy-7686b8c794-fvfq9   1/1   Running   1     23m`

`webapp-deploy-7686b8c794-kzjrx   1/1   Running   1     23m`

Kubernetes executed the liveness probe against the replicas and the two being unhealthy were killed and restarted.


Let's try right now the readiness probe. The output of this experiment should be the application not being able to return any response, because although the pods are healthy and alive, they are gonna report as non ready.

Execute the following command to scale to 0 replicas the sqlserver deployment and get the pod deleted:

`kubectl scale deployment sqlserver --replicas=0`

you should see the console output: deployment.extensions/sqlserver scaled

Wait now for the readiness probe to execute and see the pods status (kubectl get pods):

```
webapp-deploy-7686b8c794-w2nlk   0/1   Running   0     27m
webapp-deploy-7686b8c794-fvfq9   0/1   Running   1     27m
webapp-deploy-7686b8c794-kzjrx   0/1   Running   1     27m
```

As you can see, none of them are ready (0/1) and if we try to query the health of the deployment using the previous command:

`curl http://localhost:30000/self`

We will not receive any response:

curl: (7) Failed to connect to 192.168.99.100 port 30000: Connection refused


The pods are alive but one of their dependencies isn't.

In a real scenario, maybe one, maybe some, but not all the pods will be ready, and that pods will get out of traffic, but as we are using the same images and dependencies on this deployment all pods are expelled from service.

This has been a little introduction to how can you use Health Checks and benefit from it when you are running containers within an orchestrator.





