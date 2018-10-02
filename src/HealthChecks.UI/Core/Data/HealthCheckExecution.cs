using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Core.Data
{
    internal class HealthCheckExecution
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public bool IsHealthy { get; set; }

        public DateTime OnStateFrom { get; set; }

        public DateTime LastExecuted { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        public string Result { get; set; }

        public string DiscoveryService { get; set; }

        public List<HealthCheckExecutionHistory> History { get; set; }
    }
}
