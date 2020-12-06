using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddPostgreSqlStorage(this HealthChecksUIBuilder builder, string connectionString, Action<HealthCheckDbOptions> healthchecksContextOptions = null, Action<DbContextOptionsBuilder> configureOptions = null)
        {
            var hcContextOptions = new HealthCheckDbOptions();
            healthchecksContextOptions?.Invoke(hcContextOptions);

            builder.Services.AddSingleton(hcContextOptions);

            builder.Services.AddDbContext<HealthChecksDb>(options =>
            {
                configureOptions?.Invoke(options);
                options.UseNpgsql(connectionString, o => o.MigrationsAssembly("HealthChecks.UI.PostgreSQL.Storage"));
            });

            return builder;
        }
    }
}
