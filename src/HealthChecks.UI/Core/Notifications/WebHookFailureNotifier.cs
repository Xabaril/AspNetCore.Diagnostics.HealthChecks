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
    class WebHookFailureNotifier
        : IHealthCheckFailureNotifier
    {
        private readonly ILogger<WebHookFailureNotifier> _logger;
        private readonly Settings _settings;
        private readonly HealthChecksDb _db;

        public WebHookFailureNotifier(HealthChecksDb db, IOptions<Settings> settings, ILogger<WebHookFailureNotifier> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _settings = settings.Value ?? new Settings();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task NotifyDown(string livenessName, string message)
        {
            await Notify(livenessName, failure: message, isUp: false);
        }

        public async Task NotifyWakeUp(string livenessName)
        {
            await Notify(livenessName, isUp: true);
        }

        private async Task Notify(string livenessName, string failure = "", bool isUp = false)
        {
            foreach (var webHook in _settings.Webhooks)
            {
                var payload = isUp ? webHook.RestoredPayload : webHook.Payload;

                payload = payload.Replace(Keys.LIVENESS_BOOKMARK, livenessName);

                if (!await IsNotifiedOnWindowTime(livenessName, isUp))
                {
                    payload = payload.Replace(Keys.FAILURE_BOOKMARK, failure);

                    await SaveNotification(new HealthCheckFailureNotification()
                    {
                        LastNotified = DateTime.UtcNow,
                        HealthCheckName = livenessName,
                        IsUpAndRunning = isUp
                    });

                    await SendRequest(webHook.Uri, webHook.Name, payload);
                }
                else
                {
                    _logger.LogInformation("Notification is sent on same window time.");
                }
            }
        }
        private async Task<bool> IsNotifiedOnWindowTime(string livenessName, bool restore)
        {
            var lastNotification = await _db.Failures
                .Where(lf => lf.HealthCheckName.Equals(livenessName, StringComparison.InvariantCultureIgnoreCase))
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
                await _db.Failures.AddAsync(notification);
                await _db.SaveChangesAsync();
            }
        }

        private async Task SendRequest(string uri, string name, string payloadContent)
        {
            if (uri == null || !Uri.TryCreate(uri, UriKind.Absolute, out Uri webHookUri))
            {
                _logger.LogWarning($"The web hook notification uri is not stablished or is not an absolute Uri ({name}). Set the webhook uri value on BeatPulse setttings.");

                return;
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var payload = new StringContent(payloadContent, Encoding.UTF8, Keys.DEFAULT_RESPONSE_CONTENT_TYPE);
                    var response = await httpClient.PostAsync(webHookUri, payload);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"The webhook notification has not executed successfully for {name} webhook. The error code is {response.StatusCode}.");
                    }
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"The failure notification for {name} has not executed successfully.", exception);
            }
        }
    }
}
