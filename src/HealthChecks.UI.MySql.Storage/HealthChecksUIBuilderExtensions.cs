using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class HealthChecksUIBuilderExtensions
{
    // Proje Oracle'ın MySql.EntityFrameworkCore sağlayıcısına geçti (Pomelo'nun EF Core 10 sürümü yok);
    // bu sağlayıcıda UseMySQL sunucu sürümü almaz ve seçenek tipi MySQLDbContextOptionsBuilder'dır.
    public static HealthChecksUIBuilder AddMySqlStorage(
        this HealthChecksUIBuilder builder,
        string connectionString,
        Action<DbContextOptionsBuilder>? configureOptions = null,
        Action<MySQLDbContextOptionsBuilder>? configureMySqlOptions = null)
    {
        builder.Services.AddDbContext<HealthChecksDb>(options =>
        {
            configureOptions?.Invoke(options);
            options.UseMySQL(connectionString, mySqlOptionsBuilder =>
            {
                mySqlOptionsBuilder.MigrationsAssembly("HealthChecks.UI.MySql.Storage");
                configureMySqlOptions?.Invoke(mySqlOptionsBuilder);
            });
        });

        return builder;
    }
}
