using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthChecks.UI.Core.Data.Configuration
{
    class HealthCheckExecutionEntryMap
        : IEntityTypeConfiguration<HealthCheckExecutionEntry>
    {
        public void Configure(EntityTypeBuilder<HealthCheckExecutionEntry> builder)
        {
            builder.Property(le => le.Duration)
                .IsRequired(true);

            builder.Property(le => le.Status)
               .IsRequired(true);

            builder.Property(le => le.Description)
                .IsRequired(false);

            builder.Property(le => le.Name)
               .HasMaxLength(500)
               .IsRequired(true);
        }
    }
}
