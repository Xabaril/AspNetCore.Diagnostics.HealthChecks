using FluentAssertions;
using HealthChecks.UI;
using HealthChecks.UI.Core.Discovery.K8S;
using k8s.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.UI.Kubernetes
{
    public class kubernetes_address_factory_should
    {
        [Fact]
        public async Task parse_properly_the_k8s_api_discovered_services_for_a_local_cluster()
        {
            var healthPath = "healthz";
            var apiResponse = await File.ReadAllTextAsync("UI/Kubernetes/SampleData/local-cluster-discovery-sample.json");

            var services = JsonConvert.DeserializeObject<V1ServiceList>(apiResponse);

            var addressFactory = new KubernetesAddressFactory(new KubernetesDiscoverySettings
            {
                HealthPath = healthPath,
                ServicesPathAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION,
                ServicesPortAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION,
                ServicesSchemeAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION
            });

            IReadOnlyList<string> serviceAddresses = services.Items.Select(service => addressFactory.CreateAddress(service)).ToList();

            serviceAddresses[0].Should().Be("http://localhost:10000/healthz");
            serviceAddresses[1].Should().Be("http://localhost:9000/healthz");
            serviceAddresses[2].Should().Be("http://localhost:30000/healthz");
            serviceAddresses[3].Should().Be("http://10.97.1.153/healthz");
            serviceAddresses[4].Should().Be("http://[2001:0db8:85a3:0000:0000:8a2e:0370:7334]:7070/custom/health/path");
            serviceAddresses[5].Should().Be("https://some.external-site.com/custom/health/path");
        }

        [Fact]
        public async Task parse_properly_the_k8s_api_discovered_services_for_a_remote_cluster()
        {
            var healthPath = "healthz";
            var apiResponse = await File.ReadAllTextAsync("UI/Kubernetes/SampleData/remote-cluster-discovery-sample.json");

            var services = JsonConvert.DeserializeObject<V1ServiceList>(apiResponse);

            var addressFactory = new KubernetesAddressFactory(new KubernetesDiscoverySettings
            {
                HealthPath = healthPath,
                ServicesPathAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION,
                ServicesPortAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION,
                ServicesSchemeAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION
            });

            IReadOnlyList<string> serviceAddresses = services.Items.Select(service => addressFactory.CreateAddress(service)).ToList();

            serviceAddresses[0].Should().Be("http://13.73.139.23/healthz");
            serviceAddresses[1].Should().Be("http://13.80.181.10:51000/healthz");
            serviceAddresses[2].Should().Be("http://12.0.0.190:5672/healthz");
            serviceAddresses[3].Should().Be("http://12.0.0.168:30478/healthz");
            serviceAddresses[4].Should().Be("https://10.152.183.35:8080/custom/health/path");
        }

        [Fact]
        public async Task parse_properly_the_k8s_api_discovered_services_for_dns_names()
        {
            var healthPath = "healthz";
            var apiResponse = await File.ReadAllTextAsync("UI/Kubernetes/SampleData/local-cluster-discovery-sample.json");

            var services = JsonConvert.DeserializeObject<V1ServiceList>(apiResponse);

            var addressFactory = new KubernetesAddressFactory(new KubernetesDiscoverySettings
            {
                HealthPath = healthPath,
                ServicesPathAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION,
                ServicesPortAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION,
                ServicesSchemeAnnotation = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION,
                UseDNSNames = true
            });

            IReadOnlyList<string> serviceAddresses = services.Items.Select(service => addressFactory.CreateAddress(service)).ToList();

            serviceAddresses[0].Should().Be("http://localhost:10000/healthz");
            serviceAddresses[1].Should().Be("http://localhost:9000/healthz");
            serviceAddresses[2].Should().Be("http://localhost:30000/healthz");
            serviceAddresses[3].Should().Be("http://webapp4.default/healthz");
            serviceAddresses[4].Should().Be("http://seq.default:7070/custom/health/path");
            serviceAddresses[5].Should().Be("https://some.external-site.com/custom/health/path");
        }

    }
}