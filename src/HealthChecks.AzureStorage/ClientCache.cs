using System.Collections.Concurrent;

namespace HealthChecks.AzureStorage;

internal static class ClientCache<T>
{
    private static readonly ConcurrentDictionary<string, T> _cache = new();

    public static T GetOrAdd(string key, Func<string, T> clientFactory)
    {
        return _cache.GetOrAdd(key, clientFactory);
    }
}
