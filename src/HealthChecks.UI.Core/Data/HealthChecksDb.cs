using HealthChecks.UI.Core.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Core.Data
{
    public class HealthChecksDb
        : DbContext
    {
        public DbSet<HealthCheckConfiguration> Configurations { get; set; }

        public DbSet<HealthCheckExecution> Executions { get; set; }

        public DbSet<HealthCheckFailureNotification> Failures { get; set; }

        public DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries { get; set; }

        public DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories { get; set; }

        public HealthChecksDb(DbContextOptions<HealthChecksDb> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new HealthCheckConfigurationMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionEntryMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionHistoryMap());
            modelBuilder.ApplyConfiguration(new HealthCheckFailureNotificationsMap());
        }
    }
}
