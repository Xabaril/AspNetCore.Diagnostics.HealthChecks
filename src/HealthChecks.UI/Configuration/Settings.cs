using HealthChecks.UI.Core;

namespace HealthChecks.UI.Configuration
{
    public class Settings
    {
        internal List<HealthCheckSetting> HealthChecks { get; set; } = new List<HealthCheckSetting>();
        internal List<WebHookNotification> Webhooks { get; set; } = new List<WebHookNotification>();
        internal bool DisableMigrations { get; set; } = false;
        internal int MaximumExecutionHistoriesPerEndpoint { get; private set; } = 100;
        internal int EvaluationTimeInSeconds { get; set; } = 10;
        internal int ApiMaxActiveRequests { get; private set; } = 3;
        internal int MinimumSecondsBetweenFailureNotifications { get; set; } = 60 * 10;
        internal Func<IServiceProvider, HttpMessageHandler>? ApiEndpointHttpHandler { get; private set; }
        internal Action<IServiceProvider, HttpClient>? ApiEndpointHttpClientConfig { get; private set; }
        internal Dictionary<string, Type> ApiEndpointDelegatingHandlerTypes { get; set; } = new();
        internal Func<IServiceProvider, HttpMessageHandler>? WebHooksEndpointHttpHandler { get; private set; }
        internal Action<IServiceProvider, HttpClient>? WebHooksEndpointHttpClientConfig { get; private set; }
        internal Dictionary<string, Type> WebHooksEndpointDelegatingHandlerTypes { get; set; } = new Dictionary<string, Type>();
        internal string HeaderText { get; private set; } = "Health Checks Status";

        public Settings AddHealthCheckEndpoint(string name, string uri)
        {
            HealthChecks.Add(new HealthCheckSetting
            {
                Name = name,
                Uri = uri
            });

            return this;
        }

        public Settings AddWebhookNotification(string name, string uri, string payload, string restorePayload = "", Func<string, UIHealthReport, bool>? shouldNotifyFunc = null, Func<string, UIHealthReport, string>? customMessageFunc = null, Func<string, UIHealthReport, string>? customDescriptionFunc = null)
        {
            Webhooks.Add(new WebHookNotification
            {
                Name = name,
                Uri = uri,
                Payload = payload,
                RestoredPayload = restorePayload,
                ShouldNotifyFunc = shouldNotifyFunc,
                CustomMessageFunc = customMessageFunc,
                CustomDescriptionFunc = customDescriptionFunc
            });
            return this;
        }

        public Settings DisableDatabaseMigrations()
        {
            DisableMigrations = true;
            return this;
        }
        public Settings SetEvaluationTimeInSeconds(int seconds)
        {
            EvaluationTimeInSeconds = seconds;
            return this;
        }

        public Settings SetApiMaxActiveRequests(int apiMaxActiveRequests)
        {
            ApiMaxActiveRequests = apiMaxActiveRequests;
            return this;
        }

        public Settings SetHeaderText(string text)
        {
            HeaderText = string.IsNullOrEmpty(text) ? HeaderText : text;
            return this;
        }

        public Settings SetMinimumSecondsBetweenFailureNotifications(int seconds)
        {
            MinimumSecondsBetweenFailureNotifications = seconds;
            return this;
        }

        public Settings UseApiEndpointHttpMessageHandler(Func<IServiceProvider, HttpClientHandler> apiEndpointHttpHandler)
        {
            ApiEndpointHttpHandler = apiEndpointHttpHandler;
            return this;
        }

        public Settings UseApiEndpointDelegatingHandler<T>() where T : DelegatingHandler
        {
            Type delegatingHandlerType = typeof(T);

            ApiEndpointDelegatingHandlerTypes.TryAdd(delegatingHandlerType.FullName!, delegatingHandlerType);

            return this;
        }

        public Settings UseWebhookEndpointHttpMessageHandler(Func<IServiceProvider, HttpClientHandler> webhookEndpointHttpHandler)
        {
            WebHooksEndpointHttpHandler = webhookEndpointHttpHandler;
            return this;
        }

        public Settings UseWebHooksEndpointDelegatingHandler<T>() where T : DelegatingHandler
        {
            var delegatingHandlerType = typeof(T);

            WebHooksEndpointDelegatingHandlerTypes.TryAdd(delegatingHandlerType.FullName!, delegatingHandlerType);

            return this;
        }

        public Settings ConfigureApiEndpointHttpclient(Action<IServiceProvider, HttpClient> apiEndpointHttpClientconfig)
        {
            ApiEndpointHttpClientConfig = apiEndpointHttpClientconfig;
            return this;
        }

        public Settings ConfigureWebhooksEndpointHttpclient(Action<IServiceProvider, HttpClient> webhooksEndpointHttpClientconfig)
        {
            WebHooksEndpointHttpClientConfig = webhooksEndpointHttpClientconfig;
            return this;
        }

        public Settings MaximumHistoryEntriesPerEndpoint(int maxValue)
        {
            MaximumExecutionHistoriesPerEndpoint = maxValue;
            return this;
        }
    }

    public class HealthCheckSetting
    {
        public string Name { get; set; } = null!;
        public string Uri { get; set; } = null!;
    }

    public class WebHookNotification
    {
        public string Name { get; set; } = null!;
        public string Uri { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public string RestoredPayload { get; set; } = null!;
        internal Func<string, UIHealthReport, bool>? ShouldNotifyFunc { get; set; }
        internal Func<string, UIHealthReport, string>? CustomMessageFunc { get; set; }
        internal Func<string, UIHealthReport, string>? CustomDescriptionFunc { get; set; }
    }
}
