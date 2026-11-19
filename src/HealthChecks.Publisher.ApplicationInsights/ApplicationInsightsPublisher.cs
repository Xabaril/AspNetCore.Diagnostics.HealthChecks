using System.Reflection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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

    public ApplicationInsightsPublisher(
        IOptions<TelemetryConfiguration>? telemetryConfiguration,
        string? connectionString = default,
        bool saveDetailedReport = false,
        bool excludeHealthyReports = false)
    {
        _telemetryConfiguration = telemetryConfiguration?.Value;
        _connectionString = connectionString;
        _saveDetailedReport = saveDetailedReport;
        _excludeHealthyReports = excludeHealthyReports;
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
            var eventTelemetry = new EventTelemetry($"{EVENT_NAME}:{reportEntry.Key}");
            eventTelemetry.Properties[nameof(Environment.MachineName)] = Environment.MachineName;
            eventTelemetry.Properties[nameof(Assembly)] = Assembly.GetEntryAssembly()?.GetName().Name;
            eventTelemetry.Properties[HEALTHCHECK_NAME] = reportEntry.Key;
            client.TrackEvent(eventTelemetry);
            TrackMetrics(client, eventTelemetry.Properties, reportEntry.Value.Status == HealthStatus.Healthy ? 1 : 0, reportEntry.Value.Duration.TotalMilliseconds);
        }

        foreach (var reportEntry in report.Entries.Where(entry => entry.Value.Exception != null))
        {
            var exceptionTelemetry = new ExceptionTelemetry(reportEntry.Value.Exception!);
            exceptionTelemetry.Properties[nameof(Environment.MachineName)] = Environment.MachineName;
            exceptionTelemetry.Properties[nameof(Assembly)] = Assembly.GetEntryAssembly()?.GetName().Name;
            exceptionTelemetry.Properties[HEALTHCHECK_NAME] = reportEntry.Key;
            client.TrackException(exceptionTelemetry);
            TrackMetrics(client, exceptionTelemetry.Properties, reportEntry.Value.Status == HealthStatus.Healthy ? 1 : 0, reportEntry.Value.Duration.TotalMilliseconds);
        }
    }

    private static void SaveGeneralizedReport(HealthReport report, TelemetryClient client)
    {
        var eventTelemetry = new EventTelemetry(EVENT_NAME);
        eventTelemetry.Properties[nameof(Environment.MachineName)] = Environment.MachineName;
        eventTelemetry.Properties[nameof(Assembly)] = Assembly.GetEntryAssembly()?.GetName().Name;
        client.TrackEvent(eventTelemetry);
        TrackMetrics(client, eventTelemetry.Properties, report.Status == HealthStatus.Healthy ? 1 : 0, report.TotalDuration.TotalMilliseconds);
    }

    // ApplicationInsights 3.x'te EventTelemetry/ExceptionTelemetry.Metrics kalktı;
    // durum ve süre, olayla aynı özelliklerle ayrı metrik telemetrisi olarak gönderilir.
    private static void TrackMetrics(TelemetryClient client, IDictionary<string, string> properties, double status, double durationMilliseconds)
    {
        client.TrackMetric(METRIC_STATUS_NAME, status, properties);
        client.TrackMetric(METRIC_DURATION_NAME, durationMilliseconds, properties);
    }

    private TelemetryClient GetOrCreateTelemetryClient()
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
