using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HealthChecks.UI.Core.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.PostgreSQL.Data
{
    public class ApplicationDbContext : IdentityDbContext, IHealthChecksDb
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            if (Database.GetPendingMigrations().Count()>0)
            {
                Database.Migrate();
            }
        }

        public DbSet<HealthCheckConfiguration> Configurations { get; set; }
        public DbSet<HealthCheckExecution> Executions { get; set; }
        public DbSet<HealthCheckFailureNotification> Failures { get; set; }
        public DbSet<HealthCheckExecutionEntry> HealthCheckExecutionEntries { get; set; }
        public DbSet<HealthCheckExecutionHistory> HealthCheckExecutionHistories { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyHealthChecksDBConfiguration();
        }
    }
}
