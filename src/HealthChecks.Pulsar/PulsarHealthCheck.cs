using System.Text;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using DotPulsar.Schemas;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Pulsar;

/// <summary>
/// A health check for Pulsar cluster.
/// </summary>
public class PulsarHealthCheck : IHealthCheck, IAsyncDisposable
{
    private readonly IPulsarClient client;
    private readonly PulsarHealthCheckOptions options;
    private IProducer<string>? producer;

    public PulsarHealthCheck(IPulsarClient client, PulsarHealthCheckOptions? options)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.options = options ?? new PulsarHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (producer != null && producer.IsFinalState())
            {
                await producer.DisposeAsync().ConfigureAwait(false);
                producer = null;
            }

            if (producer == null)
            {
                var builder = client.NewProducer(new StringSchema(Encoding.UTF8));
                builder.Topic(options.Topic);

                options.Configure?.Invoke(builder);
                producer ??= builder.Create();
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (context.Registration.Timeout != Timeout.InfiniteTimeSpan)
            {
              cts.CancelAfter(context.Registration.Timeout);
            }

            var message = producer.NewMessage();
            await options.MessageSender(options, message, cts.Token).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException)
        {
          return new HealthCheckResult(context.Registration.FailureStatus);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => producer?.DisposeAsync() ?? new ValueTask();
}
