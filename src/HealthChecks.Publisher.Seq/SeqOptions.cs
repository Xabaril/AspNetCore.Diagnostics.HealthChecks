using HealthChecks.Publisher.Seq;

namespace Microsoft.Extensions.DependencyInjection;

public class SeqOptions
{
    public string Endpoint { get; set; } = null!;

    public string? ApiKey { get; set; }

    public SeqInputLevel DefaultInputLevel { get; set; }
}
