using HealthChecks.UI.Core;
using HealthChecks.UI.Data;

namespace Microsoft.Extensions.Diagnostics.HealthChecks;

public static class HealthReportExtensions
{
    public static List<HealthCheckExecutionEntry> ToExecutionEntries(this UIHealthReport report)
    {
        return report.Entries
            .Select(item =>
            {
                return new HealthCheckExecutionEntry
                {
                    Name = item.Key,
                    Status = item.Value.Status,
                    Description = item.Value.Description,
                    Duration = item.Value.Duration,
                    Tags = item.Value.Tags?.ToList() ?? null
                };
            }).ToList();
    }
}
