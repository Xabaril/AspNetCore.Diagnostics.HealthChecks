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
        internal Func<IServiceProvider, HttpMessageHandler> WebHooksEndpointHttpHandler { get; private set; }

        public Settings AddHealthCheckEndpoint(string name, string uri)
        {
            HealthChecks.Add(new HealthCheckSetting
            {
                Name = name,
                Uri = uri
            });

            return this;
        }
        
        public Settings AddWebhookNotification(string name, string uri, string payload, string restorePayload = "")
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri absoluteUri)) throw new ArgumentException($"Invalid uri: {uri}");

            Webhooks.Add(new WebHookNotification
            {
                Name = name,
                Uri = absoluteUri,
                Payload = payload,
                RestoredPayload = restorePayload
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
    }

    public class HealthCheckSetting
    {
        public string Name { get; set; }
        public string Uri { get; set; }
    }

    public class WebHookNotification
    {
        public string Name { get; set; }
        public Uri Uri { get; set; }
        public string Payload { get; set; }
        public string RestoredPayload { get; set; }
    }
}
