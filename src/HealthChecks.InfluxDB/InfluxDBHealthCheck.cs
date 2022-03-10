using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.InfluxDB
{
    public class InfluxDBHealthCheck
        : IHealthCheck, IDisposable
    {
        private readonly InfluxDBClient _influxdb_client;

        public InfluxDBHealthCheck(string url, string username, char[] password) => _influxdb_client = InfluxDBClientFactory.Create(url, username, password);

        public InfluxDBHealthCheck(string url, string token) => _influxdb_client = InfluxDBClientFactory.Create(url, token);

        public InfluxDBHealthCheck(InfluxDBClientOptions options) => _influxdb_client = InfluxDBClientFactory.Create(options);

        public InfluxDBHealthCheck(string url, string username, char[] password, string database, string retentionPolicy) => _influxdb_client = InfluxDBClientFactory.CreateV1(url, username, password, database, retentionPolicy);

        public InfluxDBHealthCheck(string influxDBConnectionString) => _influxdb_client = InfluxDBClientFactory.Create(influxDBConnectionString);

        public InfluxDBHealthCheck(InfluxDBClient influxdb_client) => _influxdb_client = influxdb_client;

        public InfluxDBHealthCheck(Uri influxDBConnectionString) => _influxdb_client = InfluxDBClientFactory.Create(influxDBConnectionString.ToString());

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
