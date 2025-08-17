using System.Text;
using System.Web;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core.Extensions;
using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core.Notifications;

internal sealed class WebHookFailureNotifier : IHealthCheckFailureNotifier, IDisposable
{
    private readonly ILogger<WebHookFailureNotifier> _logger;
    private readonly Settings _settings;
    private readonly HealthChecksDb _db;
    private readonly ServerAddressesService _serverAddressesService;
    private readonly HttpClient _httpClient;

    public WebHookFailureNotifier(
        HealthChecksDb db,
        IOptions<Settings> settings,
        ServerAddressesService serverAddressesService,
        ILogger<WebHookFailureNotifier> logger,
        IHttpClientFactory httpClientFactory)
    {
        _db = Guard.ThrowIfNull(db);
        _serverAddressesService = Guard.ThrowIfNull(serverAddressesService);
        _settings = settings.Value ?? new Settings();
        _logger = Guard.ThrowIfNull(logger);
        _httpClient = httpClientFactory.CreateClient(Keys.HEALTH_CHECK_WEBHOOK_HTTP_CLIENT_NAME);
    }

    public async Task NotifyDown(string name, UIHealthReport report)
    {
        await NotifyAsync(name, report, isHealthy: false).ConfigureAwait(false);
    }

    public async Task NotifyWakeUp(string name)
    {
        await NotifyAsync(name, null!, isHealthy: true).ConfigureAwait(false); // TODO: why null! ?
    }

    internal async Task NotifyAsync(string name, UIHealthReport report, bool isHealthy = false)
    {
        string? failure = default;
        string? description = default;

        if (!await IsNotifiedOnWindowTimeAsync(name, isHealthy).ConfigureAwait(false))
        {
            await SaveNotificationAsync(new HealthCheckFailureNotification()
            {
                LastNotified = DateTime.UtcNow,
                HealthCheckName = name,
                IsUpAndRunning = isHealthy
            }).ConfigureAwait(false);

            foreach (var webHook in _settings.Webhooks)
            {
                bool shouldNotify = webHook.ShouldNotifyFunc?.Invoke(name, report) ?? true;

                if (!shouldNotify)
                {
                    _logger.LogInformation("Webhook notification will not be sent because of user configuration");
                    continue;
                }

                if (!isHealthy)
                {
                    failure = webHook.CustomMessageFunc?.Invoke(name, report) ?? GetFailedMessageFromContent(report);
                    description = webHook.CustomDescriptionFunc?.Invoke(name, report) ?? GetFailedDescriptionsFromContent(report);
                }

                var payload = isHealthy ? webHook.RestoredPayload : webHook.Payload;
                payload = payload.Replace(Keys.LIVENESS_BOOKMARK, HttpUtility.JavaScriptStringEncode(name))
                    .Replace(Keys.FAILURE_BOOKMARK, HttpUtility.JavaScriptStringEncode(failure))
                    .Replace(Keys.DESCRIPTIONS_BOOKMARK, HttpUtility.JavaScriptStringEncode(description));

                Uri.TryCreate(webHook.Uri, UriKind.Absolute, out var absoluteUri);

                if (absoluteUri == null || !absoluteUri.IsValidHealthCheckEndpoint())
                {
                    Uri.TryCreate(_serverAddressesService.AbsoluteUriFromRelative(webHook.Uri), UriKind.Absolute, out absoluteUri);
                }

                if (absoluteUri == null)
                    throw new InvalidOperationException("Could not get absolute uri");

                await SendRequestAsync(absoluteUri, webHook.Name, payload).ConfigureAwait(false);
            }
        }
        else
        {
            _logger.LogInformation("Notification is sent on same window time.");
        }
    }

    private async Task<bool> IsNotifiedOnWindowTimeAsync(string livenessName, bool restore)
    {
#pragma warning disable RCS1155 // Use StringComparison when comparing strings.
        var lastNotification = await _db.Failures
            .Where(lf => lf.HealthCheckName.ToLower() == livenessName.ToLower())
            .OrderByDescending(lf => lf.LastNotified)
            .Take(1)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
#pragma warning restore RCS1155 // Use StringComparison when comparing strings.

        return lastNotification != null
            &&
            lastNotification.IsUpAndRunning == restore
            &&
            (DateTime.UtcNow - lastNotification.LastNotified).TotalSeconds < _settings.MinimumSecondsBetweenFailureNotifications;
    }

    private async Task SaveNotificationAsync(HealthCheckFailureNotification notification)
    {
        if (notification != null)
        {
            await _db.Failures
                .AddAsync(notification)
                .ConfigureAwait(false);

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    private async Task SendRequestAsync(Uri uri, string name, string payloadContent)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadContent, Encoding.UTF8, Keys.DEFAULT_RESPONSE_CONTENT_TYPE)
            };
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("The webhook notification has not executed successfully for {name} webhook. The error code is {statuscode}.", name, response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"The failure notification for {name} has not executed successfully.");
        }
    }

    private string GetFailedMessageFromContent(UIHealthReport healthReport)
    {
        var failedChecks = healthReport.Entries?.Values.Count(c => c.Status != UIHealthStatus.Healthy) ?? 0;
        var plural = PluralizeHealthcheck(failedChecks);

        return $"There {plural.plural} at least {failedChecks} {plural.noun} failing.";
    }

    private static string GetFailedDescriptionsFromContent(UIHealthReport healthReport)
    {
        var failedChecks = healthReport.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy);
        var plural = PluralizeHealthcheck(failedChecks.Count());

        return $"{string.Join(" , ", failedChecks.Select(f => f.Key))} {plural.noun} {plural.plural} failing";
    }

    public void Dispose()
    {
        _db?.Dispose();
        _httpClient.Dispose();
    }

    private static (string plural, string noun) PluralizeHealthcheck(int count) =>
        count > 1 ?
        ("are", "healthchecks") :
        ("is", "healthcheck");
}
