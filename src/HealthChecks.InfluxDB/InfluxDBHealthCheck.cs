using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.InfluxDB
{
    public class InfluxDBHealthCheck
        : IHealthCheck
    {
        private readonly InfluxDBClient _influxdb_client;

        public InfluxDBHealthCheck(string url, string username, char[] password) => _influxdb_client = InfluxDBClientFactory.Create(url, username, password);

        public InfluxDBHealthCheck(string url, string token) => _influxdb_client = InfluxDBClientFactory.Create(url, token);

        public InfluxDBHealthCheck(InfluxDBClientOptions options) => _influxdb_client = InfluxDBClientFactory.Create(options);

        /// <summary>
        /// CreateV1
        /// </summary>
        public InfluxDBHealthCheck(string url, string username, char[] password, string database, string retentionPolicy) => _influxdb_client = InfluxDBClientFactory.CreateV1(url, username, password, database, retentionPolicy);

        public InfluxDBHealthCheck(string influxDBConnectionString) => _influxdb_client = InfluxDBClientFactory.Create(influxDBConnectionString);

        public InfluxDBHealthCheck(InfluxDBClient influxdb_client) => _influxdb_client = influxdb_client;

        public InfluxDBHealthCheck(Uri influxDBConnectionString) => _influxdb_client = InfluxDBClientFactory.Create(influxDBConnectionString.ToString());

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var h = await _influxdb_client.HealthAsync();
                var hcr = new HealthCheckResult(h.Status == HealthCheck.StatusEnum.Pass ? HealthStatus.Healthy : HealthStatus.Unhealthy, $"{h.Name} {h.Version} {h.Message}");
                return hcr;
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
