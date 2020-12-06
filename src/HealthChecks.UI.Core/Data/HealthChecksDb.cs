using HealthChecks.UI.Core.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace HealthChecks.UI.Core.Data
{
    public class HealthChecksDb
        : DbContext
    {
        private readonly HealthCheckDbOptions _healthcheckOptions;

        public HealthChecksDb(DbContextOptions<HealthChecksDb> options, HealthCheckDbOptions healthcheckOptions) : base(options)
        {
            _healthcheckOptions = healthcheckOptions;
        }
        public DbSet<HealthCheckConfiguration> Configurations { get; set; }

        public DbSet<HealthCheckExecution> Executions { get; set; }

        public DbSet<HealthCheckFailureNotification> Failures { get; set; }

        public DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries { get; set; }

        public DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (!string.IsNullOrEmpty(_healthcheckOptions?.Schema))
            {
                modelBuilder.HasDefaultSchema(_healthcheckOptions.Schema);
            }

            modelBuilder.ApplyConfiguration(new HealthCheckConfigurationMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionEntryMap());
            modelBuilder.ApplyConfiguration(new HealthCheckExecutionHistoryMap());
            modelBuilder.ApplyConfiguration(new HealthCheckFailureNotificationsMap());
        }
    }
}
