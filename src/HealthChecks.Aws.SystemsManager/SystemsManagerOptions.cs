using Amazon;
using Amazon.Runtime;

namespace HealthChecks.Aws.SystemsManager;

public class SystemsManagerOptions
{
    public AWSCredentials? Credentials { get; set; }

    public RegionEndpoint? RegionEndpoint { get; set; }

    internal HashSet<string> Parameters { get; } = new HashSet<string>();

    /// <summary>
    /// Add a Parameter to be checked
    /// </summary>
    /// <param name="parameter">The parameter to be checked</param>
    /// <returns>Reference to the same <see cref="SystemsManagerOptions"/> to allow further configuration.</returns>
    public SystemsManagerOptions AddParameter(string parameter)
    {
        Parameters.Add(parameter);

        return this;
    }
}
