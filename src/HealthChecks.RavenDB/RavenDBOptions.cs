using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.RavenDB
{
    public class RavenDBOptions
    {
        public string? Database { get; set; }

        public string[] Urls { get; set; } = null!;

        public X509Certificate2? Certificate { get; set; }
    }
}
