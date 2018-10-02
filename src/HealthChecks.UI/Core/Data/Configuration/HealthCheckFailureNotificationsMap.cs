using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthChecks.UI.Core.Data.Configuration
{
    public class HealthCheckFailureNotificationsMap
        : IEntityTypeConfiguration<HealthCheckFailureNotification>
    {
        public void Configure(EntityTypeBuilder<HealthCheckFailureNotification> builder)
        {
            builder.Property(lf => lf.HealthCheckName)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(lf => lf.LastNotified)
                .IsRequired();

            builder.Property(lf => lf.IsUpAndRunning)
                .IsRequired();
        }
    }
}
