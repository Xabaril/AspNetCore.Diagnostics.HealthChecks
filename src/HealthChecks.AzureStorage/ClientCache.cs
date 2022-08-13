using System.Collections.Concurrent;

namespace HealthChecks.AzureStorage;

internal static class ClientCache
{
    public static T GetOrAdd<T>(string key, Func<string, T> clientFactory)
    {
        return Cache<T>.Instance.GetOrAdd(key, clientFactory);
    }

    private static class Cache<T>
    {
        public static ConcurrentDictionary<string, T> Instance { get; } = new();
    }
}
