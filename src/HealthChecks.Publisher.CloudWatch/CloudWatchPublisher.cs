using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.CloudWatch;
internal class CloudWatchPublisher : IHealthCheckPublisher, IDisposable
{
    private readonly string _serviceCheckName;
    private readonly AmazonCloudWatchClient _amazonCloudWatchClient;

    public CloudWatchPublisher(string serviceCheckName)
    {
        _serviceCheckName = serviceCheckName ?? throw new ArgumentNullException(nameof(serviceCheckName));
        _amazonCloudWatchClient ??= new AmazonCloudWatchClient();
    }

    public CloudWatchPublisher(string serviceCheckName, RegionEndpoint region, string awsAccessKeyId, string awsSecretAccessKey) : this(serviceCheckName)
    {
        _amazonCloudWatchClient = new AmazonCloudWatchClient(awsAccessKeyId, awsSecretAccessKey, region);
    }

    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var dimensions = new List<Dimension> {
            new Dimension
            {
                Name = "ServiceCheckName",
                Value = _serviceCheckName
            }
        };

        var metricDatas = new List<MetricDatum>
        {
            new MetricDatum
            {
                Dimensions = dimensions,
                MetricName = "Status",
                StatisticValues = new StatisticSet(),
                TimestampUtc = utcNow,
                Unit = StandardUnit.Count,
                Value = (double)report.Status
            },
            new MetricDatum
            {
                Dimensions = dimensions,
                MetricName = "Duration",
                StatisticValues = new StatisticSet(),
                TimestampUtc = utcNow,
                Unit = StandardUnit.Count,
                Value = (double)report.TotalDuration.TotalMilliseconds
            }
        };

        foreach (var keyedEntry in report.Entries)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            dimensions.AddRange(
                new List<Dimension>
                {
                    new Dimension
                    {
                        Name = $"entry-{keyedEntry.Key}-duration",
                        Value = keyedEntry.Value.Duration.ToString()
                    },
                    new Dimension
                    {
                        Name = $"entry-{keyedEntry.Key}-description",
                        Value = keyedEntry.Value.Description
                    }
                }
            );

            var metric = new MetricDatum
            {
                Dimensions = dimensions,
                MetricName = $"entry-{keyedEntry.Key}",
                StatisticValues = new StatisticSet(),
                TimestampUtc = utcNow,
                Unit = StandardUnit.Count,
                Value = (double)keyedEntry.Value.Status
            };

            metricDatas.Add(metric);
        }

        var putMetricDataRequest = new PutMetricDataRequest
        {
            MetricData = metricDatas,
            Namespace = "Xabaril/AspNetCoreDiagnosticsHealthChecks"
        };

        if (cancellationToken.IsCancellationRequested)
            return Task.CompletedTask;

        _amazonCloudWatchClient.PutMetricDataAsync(putMetricDataRequest, cancellationToken);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _amazonCloudWatchClient?.Dispose();
    }
}
