using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HealthChecks.Publisher.ApplicationInsights;

internal class ApplicationInsightsPublisher : IHealthCheckPublisher
{
    private const string EVENT_NAME = "AspNetCoreHealthCheck";
    private const string METRIC_STATUS_NAME = "AspNetCoreHealthCheckStatus";
    private const string METRIC_DURATION_NAME = "AspNetCoreHealthCheckDuration";
    private const string HEALTHCHECK_NAME = "AspNetCoreHealthCheckName";

    private static TelemetryClient? _client;
    private static readonly object _syncRoot = new object();
    private readonly TelemetryConfiguration? _telemetryConfiguration;
    private readonly string? _connectionString;
    private readonly bool _saveDetailedReport;
    private readonly bool _excludeHealthyReports;
    private readonly bool _trackAsAvailability;

    public ApplicationInsightsPublisher(
        IOptions<TelemetryConfiguration>? telemetryConfiguration,
        string? connectionString = default,
        bool saveDetailedReport = false,
        bool excludeHealthyReports = false,
        bool trackAsAvailability = false)
    {
        _telemetryConfiguration = telemetryConfiguration?.Value;
        _connectionString = connectionString;
        _saveDetailedReport = saveDetailedReport;
        _excludeHealthyReports = excludeHealthyReports;
        _trackAsAvailability = trackAsAvailability;
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        if (report.Status == HealthStatus.Healthy && _excludeHealthyReports)
        {
            return Task.CompletedTask;
        }

        var client = GetOrCreateTelemetryClient();

        if (_saveDetailedReport)
        {
            SaveDetailedReport(report, client);
        }
        else
        {
            SaveGeneralizedReport(report, client);
        }

        return Task.CompletedTask;
    }

    private void SaveDetailedReport(HealthReport report, TelemetryClient client)
    {
        foreach (var reportEntry in report.Entries.Where(entry => !_excludeHealthyReports || entry.Value.Status != HealthStatus.Healthy))
        {
            if( _trackAsAvailability )
            {
                client.TrackAvailability( $"{EVENT_NAME}:{reportEntry.Key}",
                    DateTimeOffset.UtcNow,
                    reportEntry.Value.Duration,
                    Environment.MachineName,
                    reportEntry.Value.Status == HealthStatus.Healthy,
                    reportEntry.Value.Exception?.Message,
                    properties: new Dictionary<string, string?>()
                    {
                        { nameof(Assembly), Assembly.GetEntryAssembly()?.GetName().Name },
                        { HEALTHCHECK_NAME, reportEntry.Key }
                    });
            }
            else
            {
                client.TrackEvent($"{EVENT_NAME}:{reportEntry.Key}",
                    properties: new Dictionary<string, string?>()
                    {
                        { nameof(Assembly), Assembly.GetEntryAssembly()?.GetName().Name },
                        { HEALTHCHECK_NAME, reportEntry.Key }
                    },
                    metrics: new Dictionary<string, double>()
                    {
                        { METRIC_STATUS_NAME, reportEntry.Value.Status == HealthStatus.Healthy ? 1 : 0 },
                        { METRIC_DURATION_NAME, reportEntry.Value.Duration.TotalMilliseconds }
                    });
            }
        }

        foreach (var reportEntry in report.Entries.Where(entry => entry.Value.Exception != null))
        {
            client.TrackException(reportEntry.Value.Exception,
                properties: new Dictionary<string, string?>()
                {
                    { nameof(Environment.MachineName), Environment.MachineName },
                    { nameof(Assembly), Assembly.GetEntryAssembly()?.GetName().Name },
                    { HEALTHCHECK_NAME, reportEntry.Key }
                },
                metrics: new Dictionary<string, double>()
                {
                    { METRIC_STATUS_NAME, reportEntry.Value.Status == HealthStatus.Healthy ? 1 : 0 },
                    { METRIC_DURATION_NAME, reportEntry.Value.Duration.TotalMilliseconds }
                });
        }
    }
    private void SaveGeneralizedReport(HealthReport report, TelemetryClient client)
    {
        if( _trackAsAvailability )
        {
            client.TrackAvailability( EVENT_NAME,
                DateTimeOffset.UtcNow,
                report.TotalDuration,
                Environment.MachineName,
                report.Status == HealthStatus.Healthy,
                properties: new Dictionary<string, string?>
                {
                    { nameof(Assembly), Assembly.GetEntryAssembly()?.GetName().Name }
                });
        }
        else
        {
            client.TrackEvent(EVENT_NAME,
                properties: new Dictionary<string, string?>
                {
                    { nameof(Environment.MachineName), Environment.MachineName },
                    { nameof(Assembly), Assembly.GetEntryAssembly()?.GetName().Name }
                },
                metrics: new Dictionary<string, double>
                {
                    { METRIC_STATUS_NAME, report.Status == HealthStatus.Healthy ? 1 : 0 },
                    { METRIC_DURATION_NAME, report.TotalDuration.TotalMilliseconds }
                });
        }
    }

    internal virtual TelemetryClient GetOrCreateTelemetryClient()
    {
        if (_client == null)
        {
            lock (_syncRoot)
            {
                if (_client == null)
                {
                    // Create TelemetryConfiguration
                    // Hierachy: _connectionString > _telemetryConfiguration
                    var configuration = (!string.IsNullOrWhiteSpace(_connectionString)
                        ? new TelemetryConfiguration { ConnectionString = _connectionString }
                        : _telemetryConfiguration)
                            ?? throw new ArgumentException("A connection string or TelemetryConfiguration must be set!");

                    _client = new TelemetryClient(configuration);
                }
            }
        }
        return _client;
    }
}
