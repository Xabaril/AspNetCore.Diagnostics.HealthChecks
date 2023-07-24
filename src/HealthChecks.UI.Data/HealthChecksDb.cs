using HealthChecks.UI.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Data
{
    public class HealthChecksDb : DbContext
    {
        public DbSet<HealthCheckConfiguration> Configurations => Set<HealthCheckConfiguration>();

        public DbSet<HealthCheckExecution> Executions => Set<HealthCheckExecution>();

        public DbSet<HealthCheckFailureNotification> Failures => Set<HealthCheckFailureNotification>();

        public DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries => Set<HealthCheckExecutionEntry>();

        public DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories => Set<HealthCheckExecutionHistory>();

        protected HealthChecksDb(DbContextOptions options) : base(options)
        {
        }

        public HealthChecksDb(DbContextOptions<HealthChecksDb> options) : base(options)
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
}
