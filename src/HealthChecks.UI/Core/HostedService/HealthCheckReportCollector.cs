using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Extensions;
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
        private readonly HttpClient _httpClient;
        private readonly ILogger<HealthCheckReportCollector> _logger;
        private readonly ServerAddressesService _serverAddressService;

        private static readonly Dictionary<int, Uri> endpointAddresses = new Dictionary<int, Uri>();

        public HealthCheckReportCollector(
            HealthChecksDb db,
            IHealthCheckFailureNotifier healthCheckFailureNotifier,
            IOptions<Settings> settings,
            IHttpClientFactory httpClientFactory,
            ILogger<HealthCheckReportCollector> logger,
            ServerAddressesService serverAddressService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _healthCheckFailureNotifier = healthCheckFailureNotifier ?? throw new ArgumentNullException(nameof(healthCheckFailureNotifier));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serverAddressService = serverAddressService ?? throw new ArgumentNullException(nameof(serverAddressService));
            _httpClient = httpClientFactory.CreateClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME);
        }
        public async Task Collect(CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("HealthReportCollector is collecting health checks results."))
            {
                var healthChecks = await _db.Configurations
                   .ToListAsync();

                foreach (var item in healthChecks.OrderBy(h => h.Id))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("HealthReportCollector has been cancelled.");
                        break;
                    }

                    var healthReport = await GetHealthReport(item);

                    if (healthReport.Status != UIHealthStatus.Healthy)
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

                _logger.LogDebug("HealthReportCollector has completed.");
            }
        }
        private async Task<UIHealthReport> GetHealthReport(HealthCheckConfiguration configuration)
        {
            var (uri, name) = configuration;

            try
            {
                var absoluteUri = GetEndpointUri(configuration);

                var response = await _httpClient.GetAsync(absoluteUri);

                return await response.As<UIHealthReport>();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"GetHealthReport threw an exception when trying to get report from {uri} configured with name {name}.");

                return UIHealthReport.CreateFrom(exception);
            }
        }

        private Uri GetEndpointUri(HealthCheckConfiguration configuration)
        {
            if (endpointAddresses.ContainsKey(configuration.Id))
            {
                return endpointAddresses[configuration.Id];
            }

            Uri.TryCreate(configuration.Uri, UriKind.Absolute, out var absoluteUri);

            if (absoluteUri == null || !absoluteUri.IsValidHealthCheckEndpoint())
            {
                Uri.TryCreate(_serverAddressService.AbsoluteUriFromRelative(configuration.Uri), UriKind.Absolute, out absoluteUri);
            }

            endpointAddresses[configuration.Id] = absoluteUri;

            return absoluteUri;
        }

        private async Task<bool> HasLivenessRecoveredFromFailure(HealthCheckConfiguration configuration)
        {
            var previous = await GetHealthCheckExecution(configuration);

            if (previous != null)
            {
                return previous.Status != UIHealthStatus.Healthy;
            }

            return false;
        }
        private async Task<HealthCheckExecution> GetHealthCheckExecution(HealthCheckConfiguration configuration)
        {
            return await _db.Executions
                .Include(le => le.History)
                .Include(le => le.Entries)
                .Where(le => le.Name == configuration.Name)
                .SingleOrDefaultAsync();
        }
        private async Task SaveExecutionHistory(HealthCheckConfiguration configuration, UIHealthReport healthReport)
        {
            _logger.LogDebug("HealthReportCollector - health report execution history saved.");

            var execution = await GetHealthCheckExecution(configuration);

            var lastExecutionTime = DateTime.UtcNow;

            if (execution != null)
            {
                
                if (execution.Uri != configuration.Uri)
                {
                    UpdateUris(execution, configuration);
                }

                if (execution.Status == healthReport.Status)
                {
                    _logger.LogDebug("HealthReport history already exists and is in the same state, updating the values.");

                    execution.LastExecuted = lastExecutionTime;
                }
                else
                {
                    SaveExecutionHistoryEntries(healthReport, execution, lastExecutionTime);
                }

                //update existing entries from new health report

                foreach (var item in healthReport.ToExecutionEntries())
                {
                    var existing = execution.Entries
                        .Where(e => e.Name == item.Name)
                        .SingleOrDefault();

                    if (existing != null)
                    {
                        existing.Status = item.Status;
                        existing.Description = item.Description;
                        existing.Duration = item.Duration;
                    }
                    else
                    {
                        execution.Entries.Add(item);
                    }
                }

                //remove old entries in existing execution not present in new health report

                foreach (var item in execution.Entries)
                {
                    var existing = healthReport.Entries
                        .ContainsKey(item.Name);

                    if (!existing)
                    {
                        var oldEntry = execution.Entries
                            .Where(t => t.Name == item.Name)
                            .SingleOrDefault();

                        _db.HealthCheckExecutionEntries
                            .Remove(oldEntry);
                    }
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

        private void UpdateUris(HealthCheckExecution execution, HealthCheckConfiguration configuration)
        {
            execution.Uri = configuration.Uri;
            endpointAddresses.Remove(configuration.Id);
        }

        private void SaveExecutionHistoryEntries(UIHealthReport healthReport, HealthCheckExecution execution, DateTime lastExecutionTime)
        {

            _logger.LogDebug("HealthCheckReportCollector already exists but on different state, updating the values.");

            foreach (var item in execution.Entries)
            {
                // If the health service is down, no entry in dictionary
                if (healthReport.Entries.TryGetValue(item.Name, out var reportEntry))
                {
                    if (item.Status != reportEntry.Status)
                    {
                        execution.History.Add(new HealthCheckExecutionHistory()
                        {
                            On = lastExecutionTime,
                            Status = reportEntry.Status,
                            Name = item.Name,
                            Description = reportEntry.Description
                        });
                    }
                }
            }

            execution.OnStateFrom = lastExecutionTime;
            execution.LastExecuted = lastExecutionTime;
            execution.Status = healthReport.Status;
        }
    }
}
