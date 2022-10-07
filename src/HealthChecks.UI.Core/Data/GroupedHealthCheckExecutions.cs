using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Core.Data
{
    /// <summary>
    /// Represents a collection of health check executions that have been grouped together.
    /// </summary>
    public class GroupedHealthCheckExecutions
    {
        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date/time on which the group was last updated.
        /// </summary>
        /// <remarks>
        /// This will be the newest date/time when one of the groups executions was updated.
        /// </remarks>
        public DateTime? LastExecuted { get; set; }

        /// <summary>
        /// Gets or sets the date/time on which the group last changed state.
        /// </summary>
        /// <remarks>
        /// This will be the newest date/time when one of the groups executions changed state.
        /// </remarks>
        public DateTime? OnStateFrom { get; set; }

        /// <summary>
        /// Gets or sets the health status of the group.
        /// </summary>
        /// <remarks>
        /// In case all executions are healthy, the group will be reported as health. In case one execution is unhealthy, the group will be reported as unhealthy.
        /// </remarks>
        public UIHealthStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the executions that have been grouped together.
        /// </summary>
        public List<HealthCheckExecution> Executions { get; set; }
    }
}
