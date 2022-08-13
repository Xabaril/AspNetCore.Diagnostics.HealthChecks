using System.Collections.Concurrent;

namespace HealthChecks.CosmosDb;

internal static class ClientCache
{
    public static T GetOrAdd<T>(string key, Func<string, T> clientFactory)
    {
        return Cache<T>.Map.GetOrAdd(key, clientFactory);
    }

    public static T GetOrAddDisposable<T>(string key, Func<string, T> clientFactory) where T : IDisposable
    {
        if (!Cache<T>.Map.TryGetValue(key, out T? value))
        {
            var client = clientFactory(key);
            if (!Cache<T>.Map.TryAdd(key, client))
            {
                client.Dispose();
                value = Cache<T>.Map[key];
            }
        }

        return value!;
    }

    private static class Cache<T>
    {
        public static ConcurrentDictionary<string, T> Map { get; } = new();
    }
}
