using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class HealthChecksUIBuilderExtensions
{
    public static HealthChecksUIBuilder AddMySqlStorage(
        this HealthChecksUIBuilder builder,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null,
        Action<MySqlDbContextOptionsBuilder>? configureMySqlOptions = null)
    {
        builder.Services.AddDbContext<HealthChecksDb>(options =>
        {
            configureOptions?.Invoke(options);
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptionsBuilder =>
            {
                mySqlOptionsBuilder.MigrationsAssembly("HealthChecks.UI.MySql.Storage");
                configureMySqlOptions?.Invoke(mySqlOptionsBuilder);
            });
        });

        return builder;
    }
}
