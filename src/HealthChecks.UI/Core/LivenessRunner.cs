using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core
{
    class LivenessRunner
        : ILivenessRunner
    {
        private readonly HealthChecksDb _db;
        private readonly IHealthCheckFailureNotifier _failureNotifier;
        private readonly Settings _settings;
        private readonly ILogger<LivenessRunner> _logger;

        public LivenessRunner(HealthChecksDb db,
            IHealthCheckFailureNotifier failureNotifier,
            IOptions<Settings> settings,
            ILogger<LivenessRunner> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _failureNotifier = failureNotifier ?? throw new ArgumentNullException(nameof(failureNotifier));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("LivenessRuner is on run method."))
            {
                var healthChecks = await _db.Configurations
                   .ToListAsync();

                foreach (var item in healthChecks)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("LivenessRunner Run is cancelled.");

                        break;
                    }

                    var (response, isHealthy) = await EvaluateLiveness(item);

                    if (isHealthy && (await HasLivenessRecoveredFromFailure(item)))
                    {
                        await _failureNotifier.NotifyWakeUp(item.Name);
                    }
                    else if (!isHealthy)
                    {
                        await _failureNotifier.NotifyDown(item.Name, GetFailedMessageFromContent(response));
                    }

                    await SaveExecutionHistory(item, response, isHealthy);
                }

                _logger.LogDebug("LivenessRuner run is completed.");
            }
        }

        protected internal virtual Task<HttpResponseMessage> PerformRequest(string uri)
        {
            return new HttpClient().GetAsync(uri);
        }

        private async Task<(string response, bool ishealthy)> EvaluateLiveness(HealthCheckConfiguration configuration)
        {
            var (uri, name) = configuration;

            try
            {
                var response = await PerformRequest(uri);

                var success = response.IsSuccessStatusCode;

                var content = await response.Content
                    .ReadAsStringAsync();

                return (content, success);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "LivenessRunner EvaluateLiveness throw the exception.");

                return (exception.Message, false);
            }
        }

        private async Task<HealthCheckExecution> GetHealthCheckExecution(HealthCheckConfiguration configuration)
        {
            return await _db.Executions
                .Include(le => le.History)
                .Where(le => le.Name.Equals(configuration.Name, StringComparison.InvariantCultureIgnoreCase))
                .SingleOrDefaultAsync();
        }

        private async Task<bool> HasLivenessRecoveredFromFailure(HealthCheckConfiguration configuration)
        {
            var previous = await GetHealthCheckExecution(configuration);

            if (previous != null)
            {
                var status = (HealthCheckStatus)Enum.Parse(typeof(HealthCheckStatus), previous.Status);

                return (status != HealthCheckStatus.Up);
            }

            return false;
        }

        private async Task SaveExecutionHistory(HealthCheckConfiguration configuration, string content, bool isHealthy)
        {
            _logger.LogDebug("LivenessRuner save a new liveness execution history.");

            var execution = await GetHealthCheckExecution(configuration);

            var currentStatus = GetDetailedStatusFromContent(isHealthy, content);
            var currentStatusName = Enum.GetName(typeof(HealthCheckStatus), currentStatus);
            var lastExecutionTime = DateTime.UtcNow;

            if (execution != null)
            {
                if (execution.Status.Equals(currentStatusName, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogDebug("LivenessExecutionHistory already exist and is in the same state, update the values.");

                    execution.LastExecuted = lastExecutionTime;
                    execution.Result = content;
                }
                else
                {
                    _logger.LogDebug("LivenessExecutionHistory already exist but on different state, update the values.");

                    execution.History.Add(new HealthCheckExecutionHistory()
                    {
                        On = lastExecutionTime,
                        Status = execution.Status
                    });

                    execution.IsHealthy = isHealthy;
                    execution.OnStateFrom = lastExecutionTime;
                    execution.LastExecuted = lastExecutionTime;
                    execution.Result = content;
                    execution.Status = currentStatusName;
                }
            }
            else
            {
                _logger.LogDebug("LivenessExecutionHistory is a new liveness execution history.");

                execution = new HealthCheckExecution()
                {
                    IsHealthy = isHealthy,
                    LastExecuted = lastExecutionTime,
                    OnStateFrom = lastExecutionTime,
                    Result = content,
                    Status = currentStatusName,
                    Name = configuration.Name,
                    Uri = configuration.Uri,
                    DiscoveryService = configuration.DiscoveryService
                };

                await _db.Executions
                    .AddAsync(execution);
            }

            await _db.SaveChangesAsync();
        }

        private HealthCheckStatus GetDetailedStatusFromContent(bool isHealthy, string content)
        {
            if (isHealthy)
            {
                return HealthCheckStatus.Up;
            }
            else
            {
                try
                {
                    var message = JsonConvert.DeserializeObject<OutputLivenessMessageResponse>(content);

                    if (message != null)
                    {
                        var selfLiveness = message.Checks
                            .Where(s => s.Name.Equals("self", StringComparison.InvariantCultureIgnoreCase))
                            .SingleOrDefault();

                        return (selfLiveness.IsHealthy) ? HealthCheckStatus.Degraded : HealthCheckStatus.Down;
                    }
                }
                catch
                {
                    //probably the request can't be performed (invalid domain,empty or unexpected message)
                    _logger.LogWarning($"The response from uri can't be parsed correctly. The response is {content}");
                }


                return HealthCheckStatus.Down;
            }
        }

        private string GetFailedMessageFromContent(string content)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<OutputLivenessMessageResponse>(content);

                var failedChecks = message.Checks
                    .Where(c => !c.IsHealthy)
                    .ToList();

                var failingLiveness = failedChecks.Select(f => f.Name)
                    .Aggregate((current, next) => $"{current},{next}");

                return $"There is at least {failedChecks.Count} liveness ({failingLiveness}) failing.";
            }
            catch
            {
                return content;
            }
        }

        private class OutputLivenessMessageResponse
        {
            public IEnumerable<LivenessResultResponse> Checks { get; set; }

            public DateTime StartedAtUtc { get; set; }

            public DateTime EndAtUtc { get; set; }

            public int Code { get; set; }

            public string Reason { get; set; }

        }

        private class LivenessResultResponse
        {
            public string Name { get; set; }

            public string Message { get; set; }

            public string Exception { get; set; }

            public long MilliSeconds { get; set; }

            public bool Run { get; set; }

            public string Path { get; set; }

            public bool IsHealthy { get; set; }
        }

        private enum HealthCheckStatus
        {
            Up = 0,
            Degraded = 1,
            Down = 2
        }
    }
}
