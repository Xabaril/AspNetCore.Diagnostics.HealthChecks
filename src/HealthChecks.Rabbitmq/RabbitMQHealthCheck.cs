using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace HealthChecks.RabbitMQ;

/// <summary>
/// A health check for RabbitMQ services.
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<RabbitMQHealthCheckOptions, Task<IConnection>> _connections = new();

    private IConnection? _connection;
    private readonly RabbitMQHealthCheckOptions _options;

    public RabbitMQHealthCheck(RabbitMQHealthCheckOptions options)
    {
        _options = Guard.ThrowIfNull(options);
        _connection = options.Connection;

        if (_connection is null && _options.ConnectionFactory is null && _options.ConnectionUri is null)
        {
            throw new ArgumentException("A connection, connection factory, or connection string must be set!", nameof(options));
        }
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using var model = await connection.CreateChannelAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private async Task<IConnection> EnsureConnectionAsync(CancellationToken cancellationToken) =>
        _connection ??= await _connections.GetOrAddAsync(_options, async options =>
        {
            var factory = options.ConnectionFactory;

            if (factory is null)
            {
                Guard.ThrowIfNull(options.ConnectionUri);
                factory = new ConnectionFactory
                {
                    Uri = options.ConnectionUri,
                    AutomaticRecoveryEnabled = true
                };

                if (options.RequestedConnectionTimeout is not null)
                {
                    ((ConnectionFactory)factory).RequestedConnectionTimeout = options.RequestedConnectionTimeout.Value;
                }

                if (options.Ssl is not null)
                {
                    ((ConnectionFactory)factory).Ssl = options.Ssl;
                }
            }

            return await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
}

internal static class ConcurrentDictionaryExtensions
{
    /// <summary>
    /// Provides an alternative to <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, Func{TKey, TValue})"/> specifically for asynchronous values. The factory method will only run once.
    /// </summary>
    public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
        this ConcurrentDictionary<TKey, Task<TValue>> dictionary,
        TKey key,
        Func<TKey, Task<TValue>> valueFactory) where TKey : notnull
    {
        while (true)
        {
            if (dictionary.TryGetValue(key, out var task))
            {
                return await task.ConfigureAwait(false);
            }

            // This is the task that we'll return to all waiters. We'll complete it when the factory is complete
            var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (dictionary.TryAdd(key, tcs.Task))
            {
                try
                {
                    var value = await valueFactory(key).ConfigureAwait(false);
                    tcs.TrySetResult(value);
                    return await tcs.Task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Make sure all waiters see the exception
                    tcs.SetException(ex);

                    // We remove the entry if the factory failed so it's not a permanent failure
                    // and future gets can retry (this could be a pluggable policy)
                    dictionary.TryRemove(key, out _);
                    throw;
                }
            }
        }
    }
}
