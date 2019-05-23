using System.Runtime.InteropServices;

namespace HealthChecks.UI.Core.Discovery.Docker
{
    class DockerDiscoverySettings
    {
        public bool Enabled { get; set; } = false;
        public string Endpoint { get; set; }
        public string HealthPath { get; set; } = Keys.HEALTHCHECKS_DEFAULT_PATH;
        public string ServicesLabelPrefix { get; set; } = Keys.HEALTHCHECKS_DEFAULT_DISCOVERY_LABEL;
        public int RefreshTimeOnSeconds { get; set; } = 300;

        public DockerDiscoverySettings()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Endpoint = "npipe://./pipe/docker_engine";
            else
                Endpoint = "unix:///var/run/docker.sock";
        }
    }
}
