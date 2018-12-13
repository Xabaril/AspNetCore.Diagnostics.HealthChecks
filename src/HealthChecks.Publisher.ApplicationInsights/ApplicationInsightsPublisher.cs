using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Publisher.ApplicationInsights
{
    class ApplicationInsightsPublisher
        :IHealthCheckPublisher
    {
        const string EVENT_NAME = "AspNetCoreHealthCheck";
        const string METRIC_STATUS_NAME = "AspNetCoreHealthCheckStatus";
        const string METRIC_DURATION_NAME = "AspNetCoreHealthCheckDuration";

        private readonly string _instrumentationKey;
        private static TelemetryClient _client;

        public ApplicationInsightsPublisher(string instrumentationKey = default)
        {
            _instrumentationKey = instrumentationKey;
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                //override instrumentation key or use default instrumentation 
                //key active on the project.

                var configuration = string.IsNullOrWhiteSpace(_instrumentationKey)
                    ? TelemetryConfiguration.Active
                    : new TelemetryConfiguration(_instrumentationKey);

                _client = new TelemetryClient(configuration);
            }

            _client.TrackEvent(EVENT_NAME,
                properties: new Dictionary<string, string>()
                {
                    {nameof(Environment.MachineName),Environment.MachineName},
                    {nameof(Assembly),Assembly.GetEntryAssembly().GetName().Name }
                },
                metrics: new Dictionary<string, double>()
                {
                    { METRIC_STATUS_NAME ,report.Status == HealthStatus.Healthy ? 1 :0},
                    { METRIC_DURATION_NAME,report.TotalDuration.TotalMilliseconds}
                });

            return Task.CompletedTask;
        }
    }
}
