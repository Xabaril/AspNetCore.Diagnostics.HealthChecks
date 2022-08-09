using Microsoft.Extensions.Options;

namespace HealthChecks.AzureStorage;

internal sealed class SimpleSnapshot<T> : IOptionsSnapshot<T> where T : class
{
    public T Value { get; }

    public SimpleSnapshot(T value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public T Get(string name)
    {
        return Value;
    }
}
