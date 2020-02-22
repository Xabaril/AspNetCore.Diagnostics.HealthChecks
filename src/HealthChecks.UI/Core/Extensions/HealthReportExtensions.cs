using HealthChecks.UI.Client;
using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Core.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Diagnostics.HealthChecks
{
    public static class HealthReportExtensions
    {
        public static void ApplyHealthChecksDBConfiguration(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new HealthCheckConfigurationMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionEntryMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionHistoryMap());
            modelBuilder.ApplyConfiguration(new HealthCheckFailureNotificationsMap());
        }
        public static List<HealthCheckExecutionEntry> ToExecutionEntries(this UIHealthReport report)
        {
            return report.Entries
                .Select(item =>
                {
                    return new HealthCheckExecutionEntry()
                    {
                        Name = item.Key,
                        Status = item.Value.Status,
                        Description  = item.Value.Description,
                        Duration = item.Value.Duration
                    };
                }).ToList();
        }
    }
}
