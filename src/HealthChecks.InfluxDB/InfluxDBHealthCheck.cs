using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.InfluxDB;

public class InfluxDBHealthCheck : IHealthCheck, IDisposable
{
    private readonly InfluxDBClient _influxDbClient;

    public InfluxDBHealthCheck(Func<InfluxDBClientOptions.Builder, InfluxDBClientOptions> _options)
    {
        _influxDbClient = new InfluxDBClient(_options.Invoke(InfluxDBClientOptions.Builder.CreateNew()));
    }

    public InfluxDBHealthCheck(InfluxDBClient influxDBClient)
    {
        _influxDbClient = influxDBClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var ready = await _influxDbClient.ReadyAsync().ConfigureAwait(false);
            bool ping = await _influxDbClient.PingAsync().ConfigureAwait(false);
            bool ok = ping && ready.Status == Ready.StatusEnum.Ready;
            if (ok)
            {
                var me = await _influxDbClient.GetUsersApi().MeAsync(cancellationToken).ConfigureAwait(false);
                return me?.Status == User.StatusEnum.Active
                    ? HealthCheckResult.Healthy($"Started:{ready.Started} Up:{ready.Up}")
                    : HealthCheckResult.Degraded($"User status is {me?.Status}.");
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Ping:{ping} Status:{ready.Status} Started:{ready.Started} Up:{ready.Up}");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, exception: ex);
        }
    }

    public void Dispose() => _influxDbClient.Dispose();
}
