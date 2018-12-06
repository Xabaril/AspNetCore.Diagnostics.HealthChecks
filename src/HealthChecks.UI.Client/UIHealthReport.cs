using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Client
{
    public class UIHealthReport
    {
        public UIHealthStatus Status { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public Dictionary<string, UIHealthReportEntry> Entries { get; }

        public UIHealthReport(Dictionary<string, UIHealthReportEntry> entries, TimeSpan totalDuration)
        {
            Entries = entries;
            TotalDuration = totalDuration;
        }
        public static UIHealthReport CreateFrom(HealthReport report)
        {
            var uiReport = new UIHealthReport(new Dictionary<string, UIHealthReportEntry>(), report.TotalDuration)
            {
                Status = (UIHealthStatus)report.Status,
            };

            foreach (var item in report.Entries)
            {
                var uiEntry = new UIHealthReportEntry()
                {
                    Data = item.Value.Data,
                    Description = item.Value.Description,
                    Duration = item.Value.Duration,
                    Status = (UIHealthStatus)item.Value.Status
                };

                if (item.Value.Exception != null)
                {
                    uiEntry.Exception = item.Value.Exception?.Message;
                    uiEntry.Description = item.Value.Exception?.Message;
                }

                uiReport.Entries.Add(item.Key, uiEntry);
            }

            return uiReport;
        }
    }

    public enum UIHealthStatus
    {
        Unhealthy = 0,
        Degraded = 1,
        Healthy = 2
    }

    public class UIHealthReportEntry
    {
        public IReadOnlyDictionary<string, object> Data { get; set; }
        public string Description { get; set; }
        public TimeSpan Duration { get; set; }
        public string Exception { get; set; }
        public UIHealthStatus Status { get; set; }
    }
}
