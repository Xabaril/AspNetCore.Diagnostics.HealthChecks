using HealthChecks.UI.Core;
using System;

namespace HealthChecks.UI.Data
{
    public class HealthCheckExecutionHistory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public UIHealthStatus Status { get; set; }

        public DateTime On { get; set; }
    }
}
