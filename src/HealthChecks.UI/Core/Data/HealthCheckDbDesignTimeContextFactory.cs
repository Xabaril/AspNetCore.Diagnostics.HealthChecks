using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI.Core.Data
{
    class HealthCheckDbDesignTimeContextFactory
        : IDesignTimeDbContextFactory<HealthChecksDb>
    {
        public HealthChecksDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HealthChecksDb>()
                .UseSqlite("Data Source=migrations");

            return new HealthChecksDb(optionsBuilder.Options);
        }
    }
}
