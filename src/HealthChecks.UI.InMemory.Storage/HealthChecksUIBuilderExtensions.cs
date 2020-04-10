using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HealthChecks.UI.InMemory.Storage
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddInMemoryUIStorage(this HealthChecksUIBuilder builder, Action<DbContextOptionsBuilder> configureOptions = null)
        {
            builder.Services.AddDbContext<HealthChecksDb>(options =>
            {
                configureOptions?.Invoke(options);
                options.UseInMemoryDatabase("HealthChecksUI");
            });

            return builder;
        }
    }
}
