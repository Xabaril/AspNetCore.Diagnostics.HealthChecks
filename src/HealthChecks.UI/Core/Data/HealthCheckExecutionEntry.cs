using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckExecutionEntry
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public HealthStatus Status { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
