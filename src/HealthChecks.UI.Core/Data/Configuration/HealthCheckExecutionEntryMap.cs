using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

            builder.Property(le => le.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, null),
                    v => JsonSerializer.Deserialize<List<string>>(v, null))

                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                            (c1, c2) => c1.SequenceEqual(c2),
                                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                            c => c.ToList()));
        }
    }
}
