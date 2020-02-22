using HealthChecks.UI.Core.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Core.Data
{
    public interface IHealthChecksDb
    {
        DbSet<HealthCheckConfiguration> Configurations { get; set; }

        DbSet<HealthCheckExecution> Executions { get; set; }

        DbSet<HealthCheckFailureNotification> Failures { get; set; }

        DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries { get; set; }
        DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories { get; set; }


    }
}
