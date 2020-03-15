using HealthChecks.UI.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HealthChecks.UI.Configuration
{
    public class Settings
    {
        internal List<HealthCheckSetting> HealthChecks { get; set; } = new List<HealthCheckSetting>();
        internal List<WebHookNotification> Webhooks { get; set; } = new List<WebHookNotification>();
        internal int EvaluationTimeInSeconds { get; set; } = 10;
        internal int MinimumSecondsBetweenFailureNotifications { get; set; } = 60 * 10;
        internal string HealthCheckDatabaseConnectionString { get; set; }
        internal Func<IServiceProvider, HttpMessageHandler> ApiEndpointHttpHandler { get; private set; }
        internal Action<IServiceProvider, HttpClient> ApiEndpointHttpClientConfig { get; private set; }
        internal Func<IServiceProvider, HttpMessageHandler> WebHooksEndpointHttpHandler { get; private set; }
        internal Action<IServiceProvider, HttpClient> WebHooksEndpointHttpClientConfig { get; private set; }

        public Settings AddHealthCheckEndpoint(string name, string uri)
        {
            HealthChecks.Add(new HealthCheckSetting
            {
                Name = name,
                Uri = uri
            });

            return this;
        }

        public Settings AddWebhookNotification(string name, string uri, string payload, string restorePayload = "", Func<UIHealthReport, bool> shouldNotifyFunc = null, Func<UIHealthReport,string> customMessageFunc = null, Func<UIHealthReport, string> customDescriptionFunc = null)
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

        public Settings SetEvaluationTimeInSeconds(int seconds)
        {
            EvaluationTimeInSeconds = seconds;
            return this;
        }

        public Settings SetMinimumSecondsBetweenFailureNotifications(int seconds)
        {
            MinimumSecondsBetweenFailureNotifications = seconds;
            return this;
        }

        public Settings SetHealthCheckDatabaseConnectionString(string connectionString)
        {
            HealthCheckDatabaseConnectionString = connectionString;
            return this;
        }

        public Settings UseApiEndpointHttpMessageHandler(Func<IServiceProvider, HttpClientHandler> apiEndpointHttpHandler)
        {
            ApiEndpointHttpHandler = apiEndpointHttpHandler;
            return this;
        }

        public Settings UseWebhookEndpointHttpMessageHandler(Func<IServiceProvider, HttpClientHandler> webhookEndpointHttpHandler)
        {
            WebHooksEndpointHttpHandler = webhookEndpointHttpHandler;
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
    }

    public class HealthCheckSetting
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class WebHookNotification
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Payload { get; set; }
        public string RestoredPayload { get; set; }
        internal Func<UIHealthReport, bool> ShouldNotifyFunc { get; set; } 
        internal Func<UIHealthReport, string> CustomMessageFunc { get; set; }
        internal Func<UIHealthReport, string> CustomDescriptionFunc { get; set; }
    }
}
