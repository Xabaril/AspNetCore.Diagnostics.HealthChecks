using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.DynamoDb
{
    public class DynamoDbHealthCheck
        : IHealthCheck
    {
        private readonly DynamoDBOptions _options;
        private readonly TimeSpan _timeout;

        public DynamoDbHealthCheck(DynamoDBOptions options, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(options.AccessKey)) throw new ArgumentNullException(nameof(DynamoDBOptions.AccessKey));
            if (string.IsNullOrEmpty(options.SecretKey)) throw new ArgumentNullException(nameof(DynamoDBOptions.SecretKey));
            if (options.RegionEndpoint == null) throw new ArgumentNullException(nameof(DynamoDBOptions.RegionEndpoint));

            _options = options;
            _timeout = timeout;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeout))
            using (cancellationToken.Register(() => timeoutCancellationTokenSource.Cancel()))
            {
                try
                {
                    var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
                    var client = new AmazonDynamoDBClient(credentials, _options.RegionEndpoint);

                    await client.ListTablesAsync(timeoutCancellationTokenSource.Token);
                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested)
                    {
                        return new HealthCheckResult(context.Registration.FailureStatus, "Timeout");
                    }
                    return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                }
            }
        }
    }
}
