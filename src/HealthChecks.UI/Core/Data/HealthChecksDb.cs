using HealthChecks.UI.Core.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.Core.Data
{
    class HealthChecksDb
        : DbContext, IHealthChecksDb
    {
        public DbSet<HealthCheckConfiguration> Configurations { get; set; }

        public DbSet<HealthCheckExecution> Executions { get; set; }

        public DbSet<HealthCheckFailureNotification> Failures { get; set; }

        public DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries { get; set; }

        public DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories { get; set; }

        public HealthChecksDb(DbContextOptions<HealthChecksDb> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=healthchecksdb");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyHealthChecksDBConfiguration();
        }
    }
}
