using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HealthChecks.UI.Core.Notifications
{
    internal class WebHookFailureNotifier
        : IHealthCheckFailureNotifier
    {
        private readonly ILogger<WebHookFailureNotifier> _logger;
        private readonly Settings _settings;
        private readonly HealthChecksDb _db;
        private readonly HttpClient _httpClient;

        public WebHookFailureNotifier(
            HealthChecksDb db,
            IOptions<Settings> settings,
            ILogger<WebHookFailureNotifier> logger,
            IHttpClientFactory httpClientFactory)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _settings = settings.Value ?? new Settings();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient(Keys.HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME);

        }
        public async Task NotifyDown(string name, UIHealthReport report)
        {
            await Notify(name, failure: GetFailedMessageFromContent(report), isHealthy: false, description: GetFailedDescriptionsFromContent(report));
        }
        public async Task NotifyWakeUp(string name)
        {
            await Notify(name, isHealthy: true);
        }
        private async Task Notify(string name, string failure = "", bool isHealthy = false, string description = null)
        {
            if (!await IsNotifiedOnWindowTime(name, isHealthy))
            {
                await SaveNotification(new HealthCheckFailureNotification()
                {
                    LastNotified = DateTime.UtcNow,
                    HealthCheckName = name,
                    IsUpAndRunning = isHealthy
                });

                foreach (var webHook in _settings.Webhooks)
                {
                    var payload = isHealthy ? webHook.RestoredPayload : webHook.Payload;
                    payload = payload.Replace(Keys.LIVENESS_BOOKMARK, name)
                        .Replace(Keys.FAILURE_BOOKMARK, failure)
                        .Replace(Keys.DESCRIPTIONS_BOOKMARK, description);

                    await SendRequest(webHook.Uri, webHook.Name, payload);
                }
            }
            else
            {
                _logger.LogInformation("Notification is sent on same window time.");
            }
        }
        private async Task<bool> IsNotifiedOnWindowTime(string livenessName, bool restore)
        {
            var lastNotification = await _db.Failures
                .Where(lf => lf.HealthCheckName.ToLower() == livenessName.ToLower())
                .OrderByDescending(lf => lf.LastNotified)
                .Take(1)
                .SingleOrDefaultAsync();

            return lastNotification != null
                &&
                lastNotification.IsUpAndRunning == restore
                &&
                (DateTime.UtcNow - lastNotification.LastNotified).TotalSeconds < _settings.MinimumSecondsBetweenFailureNotifications;
        }
        private async Task SaveNotification(HealthCheckFailureNotification notification)
        {
            if (notification != null)
            {
                await _db.Failures
                    .AddAsync(notification);

                await _db.SaveChangesAsync();
            }
        }
        private async Task SendRequest(Uri uri, string name, string payloadContent)
        {
            try
            {
                var payload = new StringContent(payloadContent, Encoding.UTF8, Keys.DEFAULT_RESPONSE_CONTENT_TYPE);
                var response = await _httpClient.PostAsync(uri, payload);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"The webhook notification has not executed successfully for {name} webhook. The error code is {response.StatusCode}.");
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"The failure notification for {name} has not executed successfully.", exception);
            }
        }
        private string GetFailedMessageFromContent(UIHealthReport healthReport)
        {
            var failedChecks = healthReport.Entries.Values
                .Count(c => c.Status != UIHealthStatus.Healthy);

            return $"There are at least {failedChecks} HealthChecks failing.";
        }
        private string GetFailedDescriptionsFromContent(UIHealthReport healthReport)
        {
            const string JOIN_SYMBOL = "|";

            var failedChecksDescription = healthReport.Entries.Values
                .Where(c => c.Status != UIHealthStatus.Healthy)
                .Select(x => x.Description)
                .Aggregate((first, after) => $"{first} {JOIN_SYMBOL} {after}");

            return failedChecksDescription;
        }
        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
        }
    }
}
