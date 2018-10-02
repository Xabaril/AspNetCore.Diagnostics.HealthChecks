using System;

namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckExecutionHistory
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public DateTime On { get; set; }
    }
}
