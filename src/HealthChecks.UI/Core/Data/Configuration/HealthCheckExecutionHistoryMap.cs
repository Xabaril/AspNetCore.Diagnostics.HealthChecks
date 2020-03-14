using System;
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
                .IsRequired(true)
                .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(le => le.Status)
                .HasMaxLength(50)
                .IsRequired(true);

            builder.Property(le => le.Name)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(le => le.Description)
               .IsRequired(false);  
        }
    }
}
