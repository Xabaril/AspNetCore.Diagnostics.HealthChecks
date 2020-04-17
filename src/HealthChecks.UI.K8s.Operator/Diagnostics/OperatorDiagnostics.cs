using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.K8s.Operator.Diagnostics
{
    internal class OperatorDiagnostics
    {
        private readonly ILogger _logger;
        public OperatorDiagnostics(ILoggerFactory loggerFactory)
        {
            _ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(nameof(K8sOperator));
        }

        public void OperatorStarting()
        {
            _logger.LogInformation("The operator is starting");
        }

        public void OperatorShuttingDown()
        {
            _logger.LogInformation("The operator is shutting down");
        }

        public void ServiceWatcherStarting(string @namespace) {
            _logger.LogInformation("Service watcher started for namespace {namespace}", @namespace);
        }

        public void ServiceWatcherStopped(string @namespace)
        {
            _logger.LogInformation("Service watcher stopped for namespace {namespace}", @namespace);
        }

        public void OperatorThrow(Exception exception)
        {
            _logger.LogError(exception, "The operator threw an unhandled exception");
        }

        public void ServiceWatcherThrow(Exception exception)
        {
            _logger.LogError(exception, "The operator service watcher threw an unhandled exception");
        }
    }
}
