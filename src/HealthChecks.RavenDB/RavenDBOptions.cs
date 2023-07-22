using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.RavenDB;

/// <summary>
/// Options for <see cref="RavenDBHealthCheck"/>.
/// </summary>
public class RavenDBOptions
{
    public string? Database { get; set; }

    public string[] Urls { get; set; } = null!;

    public X509Certificate2? Certificate { get; set; }

    public TimeSpan? RequestTimeout { get; set; }
}
