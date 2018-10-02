using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthChecks.UI.Core.Data.Configuration
{
    class HealthCheckExecutionMap
        : IEntityTypeConfiguration<HealthCheckExecution>
    {
        public void Configure(EntityTypeBuilder<HealthCheckExecution> builder)
        {
            builder.Property(le => le.OnStateFrom)
                .IsRequired(true);

            builder.Property(le => le.LastExecuted)
                .IsRequired(true);

            builder.Property(le => le.Status)
                .HasMaxLength(50)
                .IsRequired(true);

            builder.Property(le => le.IsHealthy)
               .IsRequired(true);

            builder.Property(le => le.Uri)
                .HasMaxLength(500)
                .IsRequired(true);

            builder.Property(le => le.Name)
               .HasMaxLength(500)
               .IsRequired(true);

            builder.Property(le => le.Result)
                .HasMaxLength(2000)
                .IsRequired(true);

            builder.Property(le => le.DiscoveryService)
                .HasMaxLength(50);

            builder.HasMany(le => le.History)
                .WithOne();
        }
    }
}
