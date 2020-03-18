using System.Collections.Generic;
using System.Linq;

namespace HealthChecks.UI.Core.Discovery.K8S
{
    class KubernetesDiscoverySettings
    {
        public bool Enabled { get; set; } = false;
        public string ClusterHost { get; set; }
        public string HealthPath { get; set; } = Keys.HEALTHCHECKS_DEFAULT_PATH;
        public string ServicesLabel { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_LABEL;
        public string ServicesPathAnnotation { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_ANNOTATION;
        public string ServicesPortAnnotation { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_ANNOTATION;
        public string ServicesSchemeAnnotation { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_ANNOTATION;
        public string Token { get; set; }
        public int RefreshTimeOnSeconds { get; set; } = 300;
        public List<string> Namespaces { get; set; } = new List<string>();
        public bool UseDNSNames { get; set; }
    }
}
