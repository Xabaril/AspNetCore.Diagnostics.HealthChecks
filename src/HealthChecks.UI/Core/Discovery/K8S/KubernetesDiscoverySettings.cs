namespace HealthChecks.UI.Core.Discovery.K8S
{
    class KubernetesDiscoverySettings
    {
        public bool Enabled { get; set; } = false;
        public bool InCluster { get; set; } = false;
        public string ClusterHost { get; set; }
        public string HealthPath { get; set; } = Keys.HEALTHCHECKS_DEFAULT_PATH;
        public string ServicesLabel { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_LABEL;
        public string ServicesPathLabel { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_LABEL;
        public string ServicesPortLabel { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_LABEL;
        public string ServicesSchemeLabel { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_LABEL;
        public string ServicesPathAnnotation { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PATH_LABEL;
        public string ServicesPortAnnotation { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_PORT_LABEL;
        public string ServicesSchemeAnnotation { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_SCHEME_LABEL;
        public string Token { get; set; }
        public int RefreshTimeOnSeconds { get; set; } = 300;
        public string[] Namespaces { get; set; } = new string[] {};
    }
}
