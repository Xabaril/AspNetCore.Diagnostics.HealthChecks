using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddInMemoryStorage(this HealthChecksUIBuilder builder, Action<DbContextOptionsBuilder> configureOptions = null, string databaseName = "HealthChecksUI")
        {
            var hcContextOptions = new HealthCheckDbOptions();
            hcContextOptions.DisableMigrations(true);
            builder.Services.AddSingleton(hcContextOptions);

            builder.Services.AddDbContext<HealthChecksDb>(options =>
            {
                configureOptions?.Invoke(options);
                options.UseInMemoryDatabase(databaseName);
            });

            return builder;
        }
    }
}
