using System.Reflection;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.CloudWatch;

/// <summary>
/// A health check publisher for AWS CloudWatch.
/// </summary>
internal sealed class CloudWatchPublisher : IHealthCheckPublisher, IDisposable
{
    private readonly CloudWatchOptions _options;
    private readonly List<Dimension> _dimensions;
    private readonly AmazonCloudWatchClient _amazonCloudWatchClient;

    public CloudWatchPublisher(CloudWatchOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _amazonCloudWatchClient = new AmazonCloudWatchClient(options.AwsAccessKeyId, options.AwsSecretAccessKey, options.Region);

        string serviceCheckName = options.ServiceCheckName ?? Assembly.GetEntryAssembly()?.GetName()?.Name ?? "undefined";

        _dimensions = new List<Dimension>
        {
            new Dimension
            {
                Name = serviceCheckName,
                Value = serviceCheckName
            }
        };
    }

    public CloudWatchPublisher(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region)
    {
        _amazonCloudWatchClient = new AmazonCloudWatchClient(awsAccessKeyId, awsSecretAccessKey, region);

        var serviceCheckName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "undefined";

        _dimensions = new List<Dimension>
        {
            new Dimension
            {
                Name = serviceCheckName,
                Value = serviceCheckName
            }
        };
    }

    public CloudWatchPublisher(string serviceCheckName, string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region) : this(awsAccessKeyId, awsSecretAccessKey, region)
    {
        if (string.IsNullOrEmpty(serviceCheckName))
            throw new ArgumentNullException(nameof(serviceCheckName));

        _dimensions = new List<Dimension>
        {
            new Dimension
            {
                Name = serviceCheckName,
                Value = serviceCheckName
            }
        };
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        var putMetricDataRequest = BuildCloudWatchMetricDataRequest(report);

        _ = await _amazonCloudWatchClient.PutMetricDataAsync(putMetricDataRequest, cancellationToken);
    }

    private PutMetricDataRequest BuildCloudWatchMetricDataRequest(HealthReport report)
    {
        var utcNow = DateTime.UtcNow;

        var metricDatas = new List<MetricDatum>
            {
                new MetricDatum
                {
                    Dimensions = _dimensions,
                    MetricName = "status",
                    StatisticValues = new StatisticSet(),
                    TimestampUtc = utcNow,
                    Unit = StandardUnit.Count,
                    Value = (int)report.Status
                }
            };

        var entriesMetricDatas = report.Entries.Select(keyedEntry =>
        {
            return new MetricDatum
            {
                Dimensions = _dimensions,
                MetricName = keyedEntry.Key,
                StatisticValues = new StatisticSet(),
                TimestampUtc = utcNow,
                Unit = StandardUnit.Count,
                Value = (int)keyedEntry.Value.Status
            };
        });

        metricDatas.AddRange(entriesMetricDatas);

        return new PutMetricDataRequest
        {
            MetricData = metricDatas,
            Namespace = _options.Namespace
        };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _amazonCloudWatchClient?.Dispose();
    }
}
