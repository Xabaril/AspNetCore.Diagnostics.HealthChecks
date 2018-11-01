using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.HostedService
{
    internal class HealthCheckReportCollector
        : IHealthCheckReportCollector
    {
        private readonly HealthChecksDb _db;
        private readonly IHealthCheckFailureNotifier _healthCheckFailureNotifier;
        private readonly Settings _settings;
        private readonly ILogger<HealthCheckReportCollector> _logger;

        public HealthCheckReportCollector(
            HealthChecksDb db,
            IHealthCheckFailureNotifier healthCheckFailureNotifier,
            IOptions<Settings> settings,
            ILogger<HealthCheckReportCollector> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _healthCheckFailureNotifier = healthCheckFailureNotifier ?? throw new ArgumentNullException(nameof(healthCheckFailureNotifier));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task Collect(CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("HealthReportCollector is collection health checks results."))
            {
                var healthChecks = await _db.Configurations
                   .ToListAsync();

                foreach (var item in healthChecks)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("HealthReportCollector is cancelled.");
                        break;
                    }

                    var healthReport = await GetHealthReport(item);

                    if (healthReport.Status != HealthStatus.Healthy)                       
                    {
                        await _healthCheckFailureNotifier.NotifyDown(item.Name, healthReport);
                    }
                    else
                    {
                        if (await HasLivenessRecoveredFromFailure(item))
                        {
                            await _healthCheckFailureNotifier.NotifyWakeUp(item.Name);
                        }
                    }

                    await SaveExecutionHistory(item, healthReport);
                }

                _logger.LogDebug("HealthReportCollector is completed.");
            }
        }
        protected internal virtual Task<HttpResponseMessage> PerformRequest(string uri)
        {
            return new HttpClient().GetAsync(uri);
        }
        private async Task<HealthReport> GetHealthReport(HealthCheckConfiguration configuration)
        {
            var (uri, name) = configuration;

            try
            {
                var response = await PerformRequest(uri);

                return await response.As<HealthReport>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "GetHealthReport throw the exception.");

                return new HealthReport(
                    entries: new Dictionary<string, HealthReportEntry>(),
                    totalDuration: TimeSpan.FromSeconds(0));
            }
        }
        private async Task<bool> HasLivenessRecoveredFromFailure(HealthCheckConfiguration configuration)
        {
            var previous = await GetHealthCheckExecution(configuration);

            if (previous != null)
            {
                return previous.Status != HealthStatus.Healthy;
            }

            return false;
        }
        private async Task<HealthCheckExecution> GetHealthCheckExecution(HealthCheckConfiguration configuration)
        {
            return await _db.Executions
                .Include(le => le.History)
                .Include(le=>le.Entries)
                .Where(le => le.Name.Equals(configuration.Name, StringComparison.InvariantCultureIgnoreCase))
                .SingleOrDefaultAsync();
        }
        private async Task SaveExecutionHistory(HealthCheckConfiguration configuration, HealthReport healthReport)
        {
            _logger.LogDebug("HealthReportCollector save a new health report execution history.");

            var execution = await GetHealthCheckExecution(configuration);

            var lastExecutionTime = DateTime.UtcNow;

            if (execution != null)
            {
                execution.Entries.Clear();
                execution.Entries = healthReport.ToExecutionEntries();

                if (execution.Status == healthReport.Status)
                {
                    _logger.LogDebug("HealthReport history already exist and is in the same state, update the values.");

                    execution.LastExecuted = lastExecutionTime;    
                }
                else
                {
                    _logger.LogDebug("HealthCheckReportCollector already exist but on different state, update the values.");

                    execution.History.Add(new HealthCheckExecutionHistory()
                    {
                        On = lastExecutionTime,
                        Status = execution.Status
                    });

                    execution.OnStateFrom = lastExecutionTime;
                    execution.LastExecuted = lastExecutionTime;
                    execution.Status = healthReport.Status;
                }
            }
            else
            {
                _logger.LogDebug("Creating a new HealthReport history.");

                execution = new HealthCheckExecution()
                {
                    LastExecuted = lastExecutionTime,
                    OnStateFrom = lastExecutionTime,
                    Entries = healthReport.ToExecutionEntries(),
                    Status = healthReport.Status,
                    Name = configuration.Name,
                    Uri = configuration.Uri,
                    DiscoveryService = configuration.DiscoveryService
                };

                await _db.Executions
                    .AddAsync(execution);
            }

            await _db.SaveChangesAsync();
        }
    }
}
