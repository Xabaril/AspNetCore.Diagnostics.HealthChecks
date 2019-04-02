using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.RavenDB
{
    public class RavenDBOptions
    {
        public string[] Urls { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public string Database { get; set; }
    }
}
