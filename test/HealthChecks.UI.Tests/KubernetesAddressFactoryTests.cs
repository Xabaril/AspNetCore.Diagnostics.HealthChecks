using System.Text.Json;
using HealthChecks.UI.Core.Discovery.K8S;
using k8s.Models;

namespace HealthChecks.UI.Tests;

public class kubernetes_address_factory_should
{
    [Fact]
    public void parse_properly_the_k8s_api_discovered_services_for_a_local_cluster()
    {
        var healthPath = "healthz";
        var apiResponse = File.ReadAllText("SampleData/local-cluster-discovery-sample.json");

        var services = JsonSerializer.Deserialize<V1ServiceList>(apiResponse);

        var addressFactory = new KubernetesAddressFactory(new KubernetesDiscoverySettings
        {
            HealthPath = healthPath,
            ServicesPathAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION,
            ServicesPortAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION,
            ServicesSchemeAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION
        });

        IReadOnlyList<string> serviceAddresses = services!.Items.Select(service => addressFactory.CreateAddress(service)).ToList();

        serviceAddresses[0].ShouldBe("http://localhost:10000/healthz");
        serviceAddresses[1].ShouldBe("http://localhost:9000/healthz");
        serviceAddresses[2].ShouldBe("http://localhost:30000/healthz");
        serviceAddresses[3].ShouldBe("http://10.97.1.153:80/healthz");
        serviceAddresses[4].ShouldBe("http://[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:7070/custom/health/path");
        serviceAddresses[5].ShouldBe("https://some.external-site.com:443/custom/health/path");
    }

    [Fact]
    public void parse_properly_the_k8s_api_discovered_services_for_a_remote_cluster()
    {
        var healthPath = "healthz";
        var apiResponse = File.ReadAllText("SampleData/remote-cluster-discovery-sample.json");

        var services = JsonSerializer.Deserialize<V1ServiceList>(apiResponse);

        var addressFactory = new KubernetesAddressFactory(new KubernetesDiscoverySettings
        {
            HealthPath = healthPath,
            ServicesPathAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION,
            ServicesPortAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION,
            ServicesSchemeAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION
        });

        IReadOnlyList<string> serviceAddresses = services!.Items.Select(service => addressFactory.CreateAddress(service)).ToList();

        serviceAddresses[0].ShouldBe("http://13.73.139.23:80/healthz");
        serviceAddresses[1].ShouldBe("http://13.80.181.10:51000/healthz");
        serviceAddresses[2].ShouldBe("http://12.0.0.190:5672/healthz");
        serviceAddresses[3].ShouldBe("http://12.0.0.168:30478/healthz");
        serviceAddresses[4].ShouldBe("https://10.152.183.35:8080/custom/health/path");
    }
}
