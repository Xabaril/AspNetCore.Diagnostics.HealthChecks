using Microsoft.Extensions.Diagnostics.HealthChecks;
using StatsdClient;

namespace HealthChecks.Publisher.Datadog;

internal class DatadogPublisher : IHealthCheckPublisher
{
    private readonly IDogStatsd _dogStatsd;
    private readonly string _serviceCheckName;
    private readonly string[] _defaultTags;

    public DatadogPublisher(IDogStatsd dogStatsd, string serviceCheckName, string[]? defaultTags)
    {
        _dogStatsd = Guard.ThrowIfNull(dogStatsd);
        _serviceCheckName = Guard.ThrowIfNull(serviceCheckName);
        _defaultTags = defaultTags ?? [];
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        foreach (var keyedEntry in report.Entries)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var key = keyedEntry.Key;
            var entry = keyedEntry.Value;
            Status dataDogStatus;

            switch (entry.Status)
            {
                case HealthStatus.Healthy:
                    dataDogStatus = Status.OK;
                    break;
                case HealthStatus.Degraded:
                    dataDogStatus = Status.WARNING;
                    break;
                case HealthStatus.Unhealthy:
                    dataDogStatus = Status.CRITICAL;
                    break;
                default:
                    dataDogStatus = Status.UNKNOWN;
                    break;
            }

            var tags = _defaultTags.Concat([$"check:{key}"]).ToArray();

            var message = entry.Description ?? entry.Status.ToString();
            _dogStatsd.ServiceCheck(_serviceCheckName, dataDogStatus, null, Environment.MachineName, tags, message);
            _dogStatsd.Timer(_serviceCheckName, entry.Duration.TotalMilliseconds, 1, tags);
        }

        return Task.CompletedTask;
    }
}
