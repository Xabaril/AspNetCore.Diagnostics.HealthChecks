namespace HealthChecks.UI.Core.Data
{
    public class HealthCheckExecutionEntry
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public UIHealthStatus Status { get; set; }

        public string? Description { get; set; }

        public TimeSpan Duration { get; set; }

        public List<string>? Tags { get; set; }
    }
}
