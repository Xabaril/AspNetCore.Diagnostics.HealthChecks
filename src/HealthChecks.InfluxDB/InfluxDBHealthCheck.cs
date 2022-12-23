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
            var ready = await _influxDbClient.ReadyAsync();
            var ping = await _influxDbClient.PingAsync();
            return ping && ready.Status == Ready.StatusEnum.Ready
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus, $"Status:{ready.Status} Started:{ready.Started} Up:{ready.Up}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    public void Dispose() => _influxDbClient.Dispose();
}
