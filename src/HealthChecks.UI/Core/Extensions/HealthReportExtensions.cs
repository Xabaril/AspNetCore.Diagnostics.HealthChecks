using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Diagnostics.HealthChecks
{
    public static class HealthReportExtensions
    {
        public static List<HealthCheckExecutionEntry> ToExecutionEntries(this UIHealthReport report)
        {
            return report.Entries
                .Select(item =>
                {
                    return new HealthCheckExecutionEntry()
                    {
                        Name = item.Key,
                        Status = item.Value.Status,
                        Description = item.Value.Description,
                        Duration = item.Value.Duration,
                        Data = JsonConvert.SerializeObject(item.Value.Data),
                        Tags = item.Value.Tags?.ToList() ?? null
                    };
                }).ToList();
        }
    }
}
