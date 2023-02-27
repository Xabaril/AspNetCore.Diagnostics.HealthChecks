using System.Net.Sockets;

namespace HealthChecks.Network;

public class TcpHealthCheckOptions
{
    internal List<(string host, int port)> ConfiguredHosts = new();

    /// <summary>
    /// Add a new host to check using <see cref="TcpHealthCheck"/>
    /// </summary>
    /// <param name="host">The host to check.</param>
    /// <param name="port">The port to use.</param>
    /// <returns>A <see cref="TcpHealthCheckOptions"/> to be chained.</returns>
    public TcpHealthCheckOptions AddHost(string host, int port)
    {
        ConfiguredHosts.Add((host, port));
        return this;
    }

    public bool CheckAllHosts { get; set; }

    /// <summary>
    /// Configure the address family.
    /// </summary>
    public AddressFamily AddressFamily { get; set; } = AddressFamily.InterNetwork;
}
