using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddSqliteStorage(
            this HealthChecksUIBuilder builder,
            string connectionString,
            Action<DbContextOptionsBuilder>? configureOptions = null,
            Action<SqliteDbContextOptionsBuilder>? configureSqliteOptions = null)
        {
            builder.Services.AddDbContext<HealthChecksDb>(optionsBuilder =>
            {
                configureOptions?.Invoke(optionsBuilder);
                optionsBuilder.UseSqlite(connectionString, sqliteOptionsBuilder =>
                {
                    sqliteOptionsBuilder.MigrationsAssembly("HealthChecks.UI.SQLite.Storage");
                    configureSqliteOptions?.Invoke(sqliteOptionsBuilder);
                });
            });

            return builder;
        }
    }
}
