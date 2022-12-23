using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.InfluxDB
{
    public class InfluxDBHealthCheck : IHealthCheck, IDisposable
    {
        private readonly InfluxDBClient _influxdb_client;

        public InfluxDBHealthCheck(string url, string username, string password)
        {
            _influxdb_client = new InfluxDBClient(url, username, password);
        }

        public InfluxDBHealthCheck(string url, string token)
        {
            _influxdb_client = new InfluxDBClient(url, token);
        }

        public InfluxDBHealthCheck(InfluxDBClientOptions options)
        {
            _influxdb_client = new InfluxDBClient(options);
        }

        public InfluxDBHealthCheck(string url, string username, string password, string database, string retentionPolicy)
        {
            _influxdb_client = new InfluxDBClient(url, username, password, database, retentionPolicy);
        }

        public InfluxDBHealthCheck(string influxDBConnectionString)
        {
            _influxdb_client = new InfluxDBClient(influxDBConnectionString);
        }

        public InfluxDBHealthCheck(InfluxDBClient influxdb_client)
        {
            _influxdb_client = influxdb_client;
        }

        public InfluxDBHealthCheck(Uri influxDBConnectionString)
        {
            _influxdb_client = new InfluxDBClient(influxDBConnectionString.ToString());
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
}
