using System.Collections.Generic;

namespace HealthChecks.Network
{
    public class SftpHealthCheckOptions
    {
        internal Dictionary<string, SftpConfiguration> ConfiguredHosts { get; } = new Dictionary<string, SftpConfiguration>();
        public SftpHealthCheckOptions AddHost(SftpConfiguration sftpConfiguration)
        {
            ConfiguredHosts.Add(sftpConfiguration.Host, sftpConfiguration);
            return this;
        }
    }
}
