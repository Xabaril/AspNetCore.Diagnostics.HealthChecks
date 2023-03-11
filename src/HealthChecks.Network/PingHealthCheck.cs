using System.Net.NetworkInformation;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Network;

public class PingHealthCheck : IHealthCheck
{
    private readonly PingHealthCheckOptions _options;

    public PingHealthCheck(PingHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var configuredHosts = _options.ConfiguredHosts.Values;

        try
        {
            List<string>? errorList = null;
            foreach (var (host, timeout) in configuredHosts)
            {
                using var ping = new Ping();

                var pingReply = await ping.SendPingAsync(host, timeout).ConfigureAwait(false);
                if (pingReply.Status != IPStatus.Success)
                {
                    (errorList ??= new()).Add($"Ping check for host {host} is failed with status reply:{pingReply.Status}");
                    if (!_options.CheckAllHosts)
                    {
                        break;
                    }
                }
            }

            return errorList.GetHealthState(context);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
