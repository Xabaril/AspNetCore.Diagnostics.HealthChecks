namespace HealthChecks.Hangfire
{
    public class HangfireOptions
    {
        public int? MaximumJobsFailed { get; set; }
        public int? MinimumAvailableServers { get; set; }
    }
}
