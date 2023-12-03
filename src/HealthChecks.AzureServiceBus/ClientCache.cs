using System.Collections.Concurrent;

internal static class ClientCache
{
    public static T GetOrAdd<T>(string key, Func<string, T> clientFactory) => Cache<T>.Instance.GetOrAdd(key, clientFactory);

    public static T GetOrAddDisposable<T>(string key, Func<string, T> clientFactory) where T : IDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = clientFactory(key);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            value.Dispose();
            return Cache<T>.Instance[key];
        }

        return value;
    }

    public static async ValueTask<T> GetOrAddDisposableAsync<T>(string key, Func<string, ValueTask<T>> clientFactory) where T : IDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = await clientFactory(key).ConfigureAwait(false);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            value.Dispose();
            return Cache<T>.Instance[key];
        }

        return value;
    }

    public static async ValueTask<T> GetOrAddAsyncDisposableAsync<T>(string key, Func<string, T> clientFactory) where T : IAsyncDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = clientFactory(key);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            await value.DisposeAsync().ConfigureAwait(false);
            return Cache<T>.Instance[key];
        }

        return value;
    }

    public static async ValueTask<T> GetOrAddAsyncDisposableAsync<T>(string key, Func<string, ValueTask<T>> clientFactory) where T : IAsyncDisposable
    {
        if (Cache<T>.Instance.TryGetValue(key, out var value))
            return value;

        value = await clientFactory(key).ConfigureAwait(false);

        if (!Cache<T>.Instance.TryAdd(key, value))
        {
            await value.DisposeAsync().ConfigureAwait(false);
            return Cache<T>.Instance[key];
        }

        return value;
    }

    private static class Cache<T>
    {
        public static ConcurrentDictionary<string, T> Instance { get; } = new();
    }
}
