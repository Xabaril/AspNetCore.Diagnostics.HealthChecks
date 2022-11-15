using System.Collections.Concurrent;

namespace HealthChecks.AzureServiceBus;

internal static class ClientCache
{
    public static T GetOrAdd<T>(string key, Func<string, T> clientFactory) => Cache<T>.Instance.GetOrAdd(key, clientFactory);

    public static T GetOrAddDisposable<T>(string key, Func<string, T> clientFactory) where T : IDisposable
    {
        if (!Cache<T>.Instance.TryGetValue(key, out var value))
        {
            value = clientFactory(key);
            if (!Cache<T>.Instance.TryAdd(key, value))
            {
                value.Dispose();
                value = Cache<T>.Instance[key];
            }
        }

        return value;
    }

    private static class Cache<T>
    {
        public static ConcurrentDictionary<string, T> Instance { get; } = new();
    }
}
