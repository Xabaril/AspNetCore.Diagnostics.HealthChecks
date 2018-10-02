using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthChecks.UI.Core.Data.Configuration
{
    class HealthCheckExecutionHistoryMap
        : IEntityTypeConfiguration<HealthCheckExecutionHistory>
    {
        public void Configure(EntityTypeBuilder<HealthCheckExecutionHistory> builder)
        {
            builder.Property(le => le.On)
                .IsRequired(true);

            builder.Property(le => le.Status)
                .HasMaxLength(50)
                .IsRequired(true);
        }
    }
}
