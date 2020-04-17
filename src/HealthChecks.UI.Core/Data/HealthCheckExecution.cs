using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckExecution
    {
        public int Id { get; set; }

        public UIHealthStatus Status { get; set; }

        public DateTime OnStateFrom { get; set; }

        public DateTime LastExecuted { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        public string DiscoveryService { get; set; }

        public List<HealthCheckExecutionEntry> Entries { get; set; }

        public List<HealthCheckExecutionHistory> History { get; set; }
    }
}
