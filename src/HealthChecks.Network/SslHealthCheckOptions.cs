using System.Net.Sockets;

namespace HealthChecks.Network;

public class SslHealthCheckOptions
{
    internal List<(string host, int port, int checkLeftDays)> ConfiguredHosts = new();

    /// <summary>
    /// Add a new host to check using <see cref="TcpHealthCheck"/>
    /// </summary>
    /// <param name="host">The host to check.</param>
    /// <param name="port">The port to use.</param>
    /// <param name="checkLeftDays">The check left days for ssl certificate  to expire.</param>
    /// <returns>A <see cref="SslHealthCheckOptions"/> to be chained.</returns>
    public SslHealthCheckOptions AddHost(string host, int port = 443, int checkLeftDays = 60)
    {
        ConfiguredHosts.Add((host, port, checkLeftDays));
        return this;
    }

    public SslHealthCheckOptions WithCheckAllHosts()
    {
        CheckAllHosts = true;
        return this;
    }

    /// <summary>
    /// Configure the address family.
    /// </summary>
    public AddressFamily AddressFamily { get; set; } = AddressFamily.InterNetwork;

    public bool CheckAllHosts { get; set; }
}
