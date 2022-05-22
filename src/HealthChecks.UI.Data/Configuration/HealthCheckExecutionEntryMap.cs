using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthChecks.UI.Data.Configuration
{
    internal class HealthCheckExecutionEntryMap
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

            builder.Property(le => le.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                    v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions))
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                            (c1, c2) => c1!.SequenceEqual(c2!),
                                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                            c => c.ToList()));
        }
    }
}
