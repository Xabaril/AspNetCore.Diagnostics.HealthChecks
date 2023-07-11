using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddInMemoryStorage(this HealthChecksUIBuilder builder, Action<DbContextOptionsBuilder>? configureOptions = null, string databaseName = "HealthChecksUI")
            => builder.AddInMemoryStorage<HealthChecksDb>(configureOptions, databaseName);

        public static HealthChecksUIBuilder AddInMemoryStorage<THealthChecksDb>(this HealthChecksUIBuilder builder, Action<DbContextOptionsBuilder>? configureOptions = null, string databaseName = "HealthChecksUI")
            where THealthChecksDb : HealthChecksDb
        {
            builder.Services.AddDbContext<HealthChecksDb, THealthChecksDb>(options =>
            {
                configureOptions?.Invoke(options);
                options.UseInMemoryDatabase(databaseName);
            });

            return builder;
        }
    }
}
