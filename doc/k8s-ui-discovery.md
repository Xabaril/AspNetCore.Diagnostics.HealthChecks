## Health Checks UI automatic Kubernetes services discovery

HealthChecks UI supports automatic discovery of k8s services exposing pods that have health checks endpoints. This means, you can benefit from it and avoid registering all the endpoints you want to check and let the UI discover them using the k8s api.

![k8s-discovery](./images/k8s-discovery-service.png)

The default mechanism to register the target endpoints to be queried for health is using the UI project appsettings.json and declare our named endpoints:

>Example:

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "Http and UI on single project",
        "Uri": "http://localhost:6001/healthz"
      }
    ],
    "Webhooks": [],
    "EvaluationTimeOnSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```


To enable Kubernetes discovery you just need to configure some settings inside the UI project appsettings.json:

```json
{
  "HealthChecksUI": {
    "KubernetesDiscoveryService": {
          "Enabled": true,
          "ClusterHost": "https://myaks-962d02ba.hcp.westeurope.azmk8s.io:443",
          "Token": "eyJhbGciOiJSUzI1NiIsImtpZCI6IiJ9.eyJpc3M...",      
          "HealthPath": "healthz"      
    }
  }
}
```

### Parameters description

Here are all the available parameters detailed:

| Parameter            | Description                                                                                                                                       | Default Value      |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| Enabled              | Establishes if the k8s discovery service is enabled of disabled                                                                                   | false              |
| InCluster            | The service discovery will try to load the cluster config as an in-cluster pod. ClusterHost and Token does not need to be defined if this is true | false              |
| ClusterHost          | The uri of the kubernetes cluster                                                                                                                 |                    |
| Token                | The token that will be sent to the cluster for authentication                                                                                     |                    |
| HealthPath           | The url path where the UI will call once the service is discovered                                                                                | hc                 |
| ServicesLabel        | The labeled services the UI will look for in k8s                                                                                                  | HealthChecks       |
| HealthPathLabel      | The label on a service that can override the configured url path                                                                                  | HealthChecksPath   |
| HealthPortLabel      | The label on a service to define which port to call. If the label does not exist on the service the first defined port will be used               | HealthChecksPort   |
| HealthSchemeLabel    | The label on a service to define which URI scheme to use for healthchecks. If the label does not exist on the service http will be used           | HealthChecksScheme |
| RefreshTimeOnSeconds | Healthchecks refresh time in seconds                                                                                                              | 300                |
| Namespaces           | The namespace(s) to query services in                                                                                                             | []                 |

## Labeling Services for discovery in Kubernetes

The `ServicesLabel` option provided in the discovery settings (by default HealthChecks) is the label that will be used to filter k8s services.

If you want to tag a service just execute the k8s command line tool (kubectl) using the following command:

`kubectl label service service-name HealthChecks=true`

Change `HealthChecks=true` by your configured ServiceLabel if you gave another value for it.

The `HealthPathLabel` option (by default HealthChecksPath) provides a method for services to override the default health path configured.

The `HealthPortLabel` option (by default HealthChecksPort) provides a method for services to specify which port to use for health checks. By default the first port defined on the service will be used. The label can refer to either the name of the port or the port number.

The `HealthSchemeLabel` option (by default HealthChecksScheme) provides a method for services to specify which URI scheme to use for health checks. By default the HTTP will be used as the URI scheme.

## How it works

The kubernetes service discovery will retrieve from the k8s api all the labelled services and from the metadata it will try to build the target url to query for health.

If you have exposed a deployment using for example a LoadBalancer on port 50000 and your configured HealthPath is "healthz" the target url to be queried would be : (ip/host):50000/healthz

If the InCluster option is set to true then the kubernetes service discovery will try to load the Cluster Host and the Token from the environment variables and service account token mounting that occurs by default in Kubernetes pods. If the InCluster option is set to false then the ClusterHost and Token options will need to be manually set.

If Namespaces is set only the labelled services within the specified namespace(s) will be queried. By default it will query all services in the cluster.

**NOTE**: Remember if you are using `kubectl proxy` you can configure your cluster address as http://localhost:8001.
