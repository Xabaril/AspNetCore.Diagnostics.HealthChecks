using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Extensions;
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
    internal class WebHookFailureNotifier<T> 
        : IHealthCheckFailureNotifier<T> where T : DbContext, IHealthChecksDb
    {
        private readonly ILogger  _logger;
        private readonly Settings _settings;
        private readonly ServerAddressesService _serverAddressesService;
        private readonly HttpClient _httpClient;

        public WebHookFailureNotifier(
            IOptions<Settings> settings,
            ServerAddressesService serverAddressesService,
            ILogger<WebHookFailureNotifier<T>> logger,
            IHttpClientFactory httpClientFactory)
        {
            _serverAddressesService = serverAddressesService ?? throw new ArgumentNullException(nameof(serverAddressesService));
            _settings = settings.Value ?? new Settings();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient(Keys.HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME);

        }
        public async Task NotifyDown(T _db, string name, UIHealthReport report)
        {
            await Notify(name, report, isHealthy: false);
        }
        public async Task NotifyWakeUp(T _db, string name)
        {
            await Notify(name, null, isHealthy: true);
        }
        internal async Task Notify(string name, UIHealthReport report, bool isHealthy = false)
        {
            string failure = default;
            string description = default;

            if (!await IsNotifiedOnWindowTime(name, isHealthy))
            {
                await SaveNotification(_db,new HealthCheckFailureNotification()
                {
                    LastNotified = DateTime.UtcNow,
                    HealthCheckName = name,
                    IsUpAndRunning = isHealthy
                });

                foreach (var webHook in _settings.Webhooks)
                {
                    bool shouldNotify = webHook.ShouldNotifyFunc?.Invoke(report) ?? true;

                    if (!shouldNotify) {
                        _logger.LogInformation("Webhook notification will not be sent because of user configuration");
                        continue;
                    };

                    if(!isHealthy)
                    {
                        failure = webHook.CustomMessageFunc?.Invoke(report) ?? GetFailedMessageFromContent(report);
                        description = webHook.CustomDescriptionFunc?.Invoke(report) ?? GetFailedDescriptionsFromContent(report);
                    }

                    var payload = isHealthy ? webHook.RestoredPayload : webHook.Payload;
                    payload = payload.Replace(Keys.LIVENESS_BOOKMARK, name)
                        .Replace(Keys.FAILURE_BOOKMARK, failure)
                        .Replace(Keys.DESCRIPTIONS_BOOKMARK, description);


                    Uri.TryCreate(webHook.Uri, UriKind.Absolute, out var absoluteUri);

                    if (absoluteUri == null || !absoluteUri.IsValidHealthCheckEndpoint())
                    {
                        Uri.TryCreate(_serverAddressesService.AbsoluteUriFromRelative(webHook.Uri), UriKind.Absolute, out absoluteUri);
                    }

                    await SendRequest(absoluteUri, webHook.Name, payload);
                }
            }
            else
            {
                _logger.LogInformation("Notification is sent on same window time.");
            }
        }
        private async Task<bool> IsNotifiedOnWindowTime(T _db, string livenessName, bool restore)
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
        private async Task SaveNotification(T _db, HealthCheckFailureNotification notification)
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
                    _logger.LogError("The webhook notification has not executed successfully for {name} webhook. The error code is {statuscode}.", name, response.StatusCode);
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
    }
}
