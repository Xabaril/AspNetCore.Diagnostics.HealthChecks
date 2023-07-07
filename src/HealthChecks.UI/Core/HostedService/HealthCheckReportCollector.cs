using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Core.Extensions;
using HealthChecks.UI.Core.Notifications;
using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.Core.HostedService
{
    internal class HealthCheckReportCollector : IHealthCheckReportCollector
    {
        private readonly HealthChecksDb _db;
        private readonly IHealthCheckFailureNotifier _healthCheckFailureNotifier;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HealthCheckReportCollector> _logger;
        private readonly ServerAddressesService _serverAddressService;
        private readonly IEnumerable<IHealthCheckCollectorInterceptor> _interceptors;
        private static readonly Dictionary<int, Uri> _endpointAddresses = new();
        private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                // allowIntegerValues: true https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/1422
                new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true)
            }
        };

        public HealthCheckReportCollector(
            HealthChecksDb db,
            IHealthCheckFailureNotifier healthCheckFailureNotifier,
            IHttpClientFactory httpClientFactory,
            ILogger<HealthCheckReportCollector> logger,
            ServerAddressesService serverAddressService,
            IEnumerable<IHealthCheckCollectorInterceptor> interceptors)
        {
            _db = Guard.ThrowIfNull(db);
            _healthCheckFailureNotifier = Guard.ThrowIfNull(healthCheckFailureNotifier);
            _logger = Guard.ThrowIfNull(logger);
            _serverAddressService = Guard.ThrowIfNull(serverAddressService);
            _interceptors = interceptors ?? Enumerable.Empty<IHealthCheckCollectorInterceptor>();
            _httpClient = httpClientFactory.CreateClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME);
        }

        public async Task Collect(CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("HealthReportCollector is collecting health checks results."))
            {
                var healthChecks = await _db.Configurations.ToListAsync(cancellationToken);

                foreach (var item in healthChecks.OrderBy(h => h.Id))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("HealthReportCollector has been cancelled.");
                        break;
                    }

                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnCollectExecuting(item);
                    }

                    var healthReport = await GetHealthReportAsync(item);

                    if (healthReport.Status != UIHealthStatus.Healthy)
                    {
                        if (!_settings.NotifyUnHealthyOnceTimeUntilChange || await ShouldNotifyAsync(item.Name, healthReport))
                        {
                            await _healthCheckFailureNotifier.NotifyDown(item.Name, healthReport);
                        }
                    }
                    else
                    {
                        if (await HasLivenessRecoveredFromFailureAsync(item))
                        {
                            await _healthCheckFailureNotifier.NotifyWakeUp(item.Name);
                        }
                    }

                    await SaveExecutionHistoryAsync(item, healthReport);

                    foreach (var interceptor in _interceptors)
                    {
                        await interceptor.OnCollectExecuted(healthReport);
                    }
                }

                _logger.LogDebug("HealthReportCollector has completed.");
            }
        }

        private async Task<UIHealthReport> GetHealthReportAsync(HealthCheckConfiguration configuration)
        {
            var (uri, name) = configuration;

            try
            {
                var absoluteUri = GetEndpointUri(configuration);
                HttpResponseMessage? response = null;

                if (!string.IsNullOrEmpty(absoluteUri.UserInfo))
                {
                    var userInfoArr = absoluteUri.UserInfo.Split(':');
                    if (userInfoArr.Length == 2 && !string.IsNullOrEmpty(userInfoArr[0]) && !string.IsNullOrEmpty(userInfoArr[1]))
                    {
                        //_httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(userInfoArr[0], userInfoArr[1]);

                        // To support basic auth; we can add an auth header to _httpClient, in the DefaultRequestHeaders (as above commented line).
                        // This would then be in place for the duration of the _httpClient lifetime, with the auth header present in every
                        // request. This also means every call to GetHealthReportAsync should check if _httpClient's DefaultRequestHeaders
                        // has already had auth added.
                        // Otherwise, if you don't want to effect _httpClient's DefaultRequestHeaders, then you have to explicitly create
                        // a request message (for each request) and add/set the auth header in each request message. Doing the latter
                        // means you can't use _httpClient.GetAsync and have to use _httpClient.SendAsync

                        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, absoluteUri);
                        requestMessage.Headers.Authorization = new BasicAuthenticationHeaderValue(userInfoArr[0], userInfoArr[1]);
                        response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                    }
                }

                response ??= await _httpClient.GetAsync(absoluteUri, HttpCompletionOption.ResponseHeadersRead);

                using (response)
                {
                    return await response.Content.ReadFromJsonAsync<UIHealthReport>(_options)
                        ?? throw new InvalidOperationException($"{nameof(HttpContentJsonExtensions.ReadFromJsonAsync)} returned null");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"GetHealthReport threw an exception when trying to get report from {uri} configured with name {name}.");

                return UIHealthReport.CreateFrom(exception);
            }
        }

        private Uri GetEndpointUri(HealthCheckConfiguration configuration)
        {
            if (_endpointAddresses.ContainsKey(configuration.Id))
            {
                return _endpointAddresses[configuration.Id];
            }

            Uri.TryCreate(configuration.Uri, UriKind.Absolute, out var absoluteUri);

            if (absoluteUri == null || !absoluteUri.IsValidHealthCheckEndpoint())
            {
                Uri.TryCreate(_serverAddressService.AbsoluteUriFromRelative(configuration.Uri), UriKind.Absolute, out absoluteUri);
            }

            if (absoluteUri == null)
                throw new InvalidOperationException("Could not get endpoint uri from configuration");

            _endpointAddresses[configuration.Id] = absoluteUri;

            return absoluteUri;
        }

        private async Task<bool> HasLivenessRecoveredFromFailureAsync(HealthCheckConfiguration configuration)
        {
            var previous = await GetHealthCheckExecutionAsync(configuration);

            return previous != null && previous.Status != UIHealthStatus.Healthy;
        }

        private async Task<HealthCheckExecution?> GetHealthCheckExecutionAsync(HealthCheckConfiguration configuration)
        {
            return await _db.Executions
                .Include(le => le.History)
                .Include(le => le.Entries)
                .Where(le => le.Name == configuration.Name)
                .SingleOrDefaultAsync();
        }

        private async Task<bool> ShouldNotifyAsync(string healthCheckName)

        {
            var lastNotifications = await _db.Failures
               .Where(lf => string.Equals(lf.HealthCheckName, healthCheckName, StringComparison.OrdinalIgnoreCase))
               .OrderByDescending(lf => lf.LastNotified)
               .Take(2).ToListAsync();

            if (lastNotifications == null)
            {
                return true;
            }

            if (lastNotifications.Count == 2)
            {
                var first = lastNotifications[0];
                var second = lastNotifications[1];
                if (first.IsUpAndRunning == second.IsUpAndRunning)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task SaveExecutionHistoryAsync(HealthCheckConfiguration configuration, UIHealthReport healthReport)
        {
            _logger.LogDebug("HealthReportCollector - health report execution history saved.");

            var execution = await GetHealthCheckExecutionAsync(configuration);

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

                // update existing entries with values from new health report

                foreach (var item in healthReport.ToExecutionEntries())
                {
                    var existing = execution.Entries
                        .SingleOrDefault(e => e.Name == item.Name);

                    if (existing != null)
                    {
                        existing.Status = item.Status;
                        existing.Description = item.Description;
                        existing.Duration = item.Duration;
                        existing.Tags = item.Tags;
                    }
                    else
                    {
                        execution.Entries.Add(item);
                    }
                }

                // remove old entries if existing execution not present in new health report

                foreach (var item in execution.Entries)
                {
                    if (!healthReport.Entries.ContainsKey(item.Name))
                        _db.HealthCheckExecutionEntries.Remove(item);
                }
            }
            else
            {
                _logger.LogDebug("Creating a new HealthReport history.");

                execution = new HealthCheckExecution
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

        private static void UpdateUris(HealthCheckExecution execution, HealthCheckConfiguration configuration)
        {
            execution.Uri = configuration.Uri;
            _endpointAddresses.Remove(configuration.Id);
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
                        execution.History.Add(new HealthCheckExecutionHistory
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
