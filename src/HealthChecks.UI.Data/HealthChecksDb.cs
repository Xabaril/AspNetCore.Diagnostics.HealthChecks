using HealthChecks.UI.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Data;

public class HealthChecksDb : DbContext
{
    public DbSet<HealthCheckConfiguration> Configurations { get; set; }

    public DbSet<HealthCheckExecution> Executions { get; set; }

    public DbSet<HealthCheckFailureNotification> Failures { get; set; }

    public DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries { get; set; }

    public DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories { get; set; }

    protected HealthChecksDb(DbContextOptions options) : base(options)
    {
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public HealthChecksDb(DbContextOptions<HealthChecksDb> options) : base(options)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new HealthCheckConfigurationMap());
        modelBuilder.ApplyConfiguration(new HealthCheckExecutionMap());
        modelBuilder.ApplyConfiguration(new HealthCheckExecutionEntryMap());
        modelBuilder.ApplyConfiguration(new HealthCheckExecutionHistoryMap());
        modelBuilder.ApplyConfiguration(new HealthCheckFailureNotificationsMap());
    }
}
