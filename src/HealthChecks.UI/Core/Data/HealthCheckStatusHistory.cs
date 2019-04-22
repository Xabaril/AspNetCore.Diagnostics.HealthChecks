using HealthChecks.UI.Client;
using System;

namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckExecutionHistory
    {
        public int Id { get; set; }

        public UIHealthStatus Status { get; set; }

        public DateTime On { get; set; }
    }
}
