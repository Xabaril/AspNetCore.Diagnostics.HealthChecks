
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddSqlServerStorage(this HealthChecksUIBuilder builder, string connectionString, Action<DbContextOptionsBuilder> configureOptions = null)
        {            
            builder.Services.AddDbContext<HealthChecksDb>(options =>
            {   
                configureOptions?.Invoke(options);
                options.UseSqlServer(connectionString, s => s.MigrationsAssembly("HealthChecks.UI.SqlServer.Storage"));
            });

            return builder;
        }
    }
}
