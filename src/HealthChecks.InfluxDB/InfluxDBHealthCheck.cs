using System.Collections.ObjectModel;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.InfluxDB;

public class InfluxDBHealthCheck : IHealthCheck, IDisposable
{
#pragma warning disable IDISP008 // Don't assign member with injected and created disposables
    private readonly InfluxDBClient _influxDbClient;
#pragma warning restore IDISP008 // Don't assign member with injected and created disposables
    private readonly bool _ownsClient;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.type", nameof(InfluxDBHealthCheck) },
                    { "db.system.name", "influxdb" }
    };

    public InfluxDBHealthCheck(Func<InfluxDBClientOptions.Builder, InfluxDBClientOptions> _options)
    {
        _ownsClient = true;
        _influxDbClient = new InfluxDBClient(_options.Invoke(InfluxDBClientOptions.Builder.CreateNew()));
    }

    public InfluxDBHealthCheck(InfluxDBClient influxDBClient)
    {
        _ownsClient = false;
        _influxDbClient = influxDBClient;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            var ready = await _influxDbClient.ReadyAsync().ConfigureAwait(false);
            bool ping = await _influxDbClient.PingAsync().ConfigureAwait(false);
            bool ok = ping && ready.Status == Ready.StatusEnum.Ready;
            if (ok)
            {
                var me = await _influxDbClient.GetUsersApi().MeAsync(cancellationToken).ConfigureAwait(false);
                return me?.Status == User.StatusEnum.Active
                    ? HealthCheckResult.Healthy($"Started:{ready.Started} Up:{ready.Up}", data: new ReadOnlyDictionary<string, object>(checkDetails))
                    : HealthCheckResult.Degraded($"User status is {me?.Status}.", data: new ReadOnlyDictionary<string, object>(checkDetails));
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Ping:{ping} Status:{ready.Status} Started:{ready.Started} Up:{ready.Up}", data: new ReadOnlyDictionary<string, object>(checkDetails));
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }

    public virtual void Dispose()
    {
        if (_ownsClient)
            _influxDbClient.Dispose();
    }
}
