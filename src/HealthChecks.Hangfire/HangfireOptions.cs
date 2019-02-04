namespace HealthChecks.Hangfire
{
    public class HangfireOptions
    {
        public int? MaximumFailed { get; set; }
        public int? MinimumServers { get; set; }
    }
}
