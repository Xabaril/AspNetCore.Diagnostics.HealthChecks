namespace HealthChecks.Hangfire
{
    /// <summary>
    /// Options for <see cref="HangfireHealthCheck"/>.
    /// </summary>
    public class HangfireOptions
    {
        public int? MaximumJobsFailed { get; set; }

        public int? MinimumAvailableServers { get; set; }
    }
}
