using HealthChecks.UI.Core;
using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Data
{
    public class HealthCheckExecutionEntry
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public UIHealthStatus Status { get; set; }

        public string Description { get; set; }

        public TimeSpan Duration { get; set; }

        public List<string> Tags { get; set; }
    }
}