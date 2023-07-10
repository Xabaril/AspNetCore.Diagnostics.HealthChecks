namespace HealthChecks.Publisher.Seq;

public class RawEvent
{
    public DateTimeOffset Timestamp { get; set; }

    public string Level { get; set; } = null!;

    public string MessageTemplate { get; set; } = null!;

    public string RawReport { get; set; } = null!; //TODO: remove?

    public Dictionary<string, object?> Properties { get; set; } = null!;
}
