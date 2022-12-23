using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.InfluxDB;

public class InfluxDBHealthCheck : IHealthCheck, IDisposable
{
    private readonly InfluxDBClient _influxdb_client;

    public InfluxDBHealthCheck(Func<InfluxDBClientOptions.Builder, InfluxDBClientOptions> _options)
    {
        _influxdb_client = new InfluxDBClient(_options.Invoke(InfluxDBClientOptions.Builder.CreateNew()));
    }

    public InfluxDBHealthCheck(InfluxDBClient influxDBClient)
    {
        _influxdb_client = influxDBClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var ready = await _influxdb_client.ReadyAsync();
            var ping = await _influxdb_client.PingAsync();
            return new HealthCheckResult(ping && ready.Status == Ready.StatusEnum.Ready ? HealthStatus.Healthy : context.Registration.FailureStatus, $"Status:{ready.Status} Started:{ready.Started} Up:{ready.Up}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    public void Dispose() => _influxdb_client.Dispose();
}
